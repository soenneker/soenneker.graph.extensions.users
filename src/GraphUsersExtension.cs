using Microsoft.Graph.Models;
using Soenneker.Extensions.String;
using System;
using System.Linq;

namespace Soenneker.Graph.Extensions.Users;

/// <summary>
/// A collection of helpful GraphUser extension methods
/// </summary>
public static class GraphUsersExtension
{
    /// <summary>
    /// Extracts contact info in one pass with early-return shortcuts.
    /// <para>**Requires** <c>$select</c> to include: <c>mail, userPrincipalName, otherMails, identities, givenName, surname, displayName</c>.</para>
    /// </summary>
    public static (string? Email, string? FirstName, string? LastName) GetEmailAndName(this User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        //--------------------------------------------------------
        // 1️⃣  Early-return: everything already populated
        //--------------------------------------------------------
        if (user.Mail.HasContent() && user.GivenName.HasContent() && user.Surname.HasContent())
            return (user.Mail!.Trim(), user.GivenName!.Trim(), user.Surname!.Trim());

        //--------------------------------------------------------
        // 2️⃣  Email – cheapest sources first
        //--------------------------------------------------------
        string? email =
            user.Mail
            ?? user.UserPrincipalName
            ?? user.OtherMails?.FirstOrDefault(m => m.HasContent());

        if (email.IsNullOrEmpty() && user.Identities is not null)
        {
            foreach (ObjectIdentity id in user.Identities)
            {
                // a) Local accounts that sign in with e-mail
                if (id.SignInType == "emailAddress")
                {
                    email = id.IssuerAssignedId;
                    break;
                }

                // b) Federated (Google, Facebook, etc.) – look for an @
                if (id.SignInType == "federated" &&
                    id.IssuerAssignedId.HasContent() && id.IssuerAssignedId.IndexOf('@') >= 0)
                {
                    email = id.IssuerAssignedId;
                    // keep looping – a later identity could be "emailAddress"
                }
            }
        }

        //--------------------------------------------------------
        // 3️⃣  First / last names
        //--------------------------------------------------------
        string? first = user.GivenName;
        string? last = user.Surname;

        if (first.IsNullOrEmpty() || last.IsNullOrEmpty())
        {
            string? dn = user.DisplayName;
            if (dn.HasContent())
            {
                ReadOnlySpan<char> span = dn.AsSpan();
                int firstSpace = span.IndexOf(' ');
                int lastSpace = span.LastIndexOf(' ');

                // Single-token displayName → treat as FirstName if missing
                if (firstSpace < 0)
                {
                    first ??= dn;
                }
                else
                {
                    // “Mary Anne van der Woodsen” ↓
                    if (first.IsNullOrEmpty())
                        first = dn[..firstSpace];

                    if (last.IsNullOrEmpty())
                        last = dn[(lastSpace + 1)..];   // last token = last name
                }
            }
        }

        //--------------------------------------------------------
        // 4️⃣  Final tidy-up
        //--------------------------------------------------------
        return (email?.Trim(), first?.Trim(), last?.Trim());
    }
}