using Microsoft.Graph.Models;
using System;
using System.Linq;

namespace Soenneker.Graph.Extensions.Users;

/// <summary>
/// A collection of helpful GraphUser extension methods
/// </summary>
public static class GraphUsersExtension
{
    /// <summary>
    /// Extracts an email and name using Graph identity fields and display-name fallbacks.
    /// <para>When the user is loaded from Graph, select <c>mail</c>, <c>userPrincipalName</c>, <c>otherMails</c>, <c>identities</c>, <c>givenName</c>, <c>surname</c>, and <c>displayName</c>.</para>
    /// </summary>
    /// <param name="user">User for the get email and name operation.</param>
    /// <returns>The best available email, first name, and last name. Each value is nullable.</returns>
    public static (string? Email, string? FirstName, string? LastName) GetEmailAndName(this User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        //--------------------------------------------------------
        // 1️⃣  Early-return: everything already populated
        //--------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(user.Mail) && !string.IsNullOrWhiteSpace(user.GivenName) && !string.IsNullOrWhiteSpace(user.Surname))
            return (user.Mail!.Trim(), user.GivenName!.Trim(), user.Surname!.Trim());

        //--------------------------------------------------------
        // 2️⃣  Email – cheapest sources first
        //--------------------------------------------------------
        string? email = !string.IsNullOrWhiteSpace(user.Mail)
            ? user.Mail
            : !string.IsNullOrWhiteSpace(user.UserPrincipalName)
                ? user.UserPrincipalName
                : user.OtherMails?.FirstOrDefault(static mail => !string.IsNullOrWhiteSpace(mail));

        if (string.IsNullOrWhiteSpace(email) && user.Identities is not null)
        {
            foreach (ObjectIdentity id in user.Identities)
            {
                // a) Local accounts that sign in with e-mail
                if (string.Equals(id.SignInType, "emailAddress", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(id.IssuerAssignedId))
                {
                    email = id.IssuerAssignedId;
                    break;
                }

                // b) Federated (Google, Facebook, etc.) – look for an @
                if (string.Equals(id.SignInType, "federated", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(id.IssuerAssignedId) && id.IssuerAssignedId.IndexOf('@') >= 0)
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

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
        {
            string? dn = user.DisplayName;
            if (!string.IsNullOrWhiteSpace(dn))
            {
                string displayName = dn.Trim();
                ReadOnlySpan<char> span = displayName.AsSpan();
                int firstSpace = span.IndexOf(' ');
                int lastSpace = span.LastIndexOf(' ');

                // Single-token displayName → treat as FirstName if missing
                if (firstSpace < 0)
                {
                    first = string.IsNullOrWhiteSpace(first) ? displayName : first;
                }
                else
                {
                    // “Mary Anne van der Woodsen” ↓
                    if (string.IsNullOrWhiteSpace(first))
                        first = displayName[..firstSpace];

                    if (string.IsNullOrWhiteSpace(last))
                        last = displayName[(lastSpace + 1)..];   // last token = last name
                }
            }
        }

        //--------------------------------------------------------
        // 4️⃣  Final tidy-up
        //--------------------------------------------------------
        return (email?.Trim(), first?.Trim(), last?.Trim());
    }
}
