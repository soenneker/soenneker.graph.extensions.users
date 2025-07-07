using Microsoft.Graph.Models;
using Soenneker.Extensions.String;
using System;

namespace Soenneker.Graph.Extensions.Users;

/// <summary>
/// A collection of helpful GraphUser extension methods
/// </summary>
public static class GraphUsersExtension
{
    /// <summary>
    /// Fast, low-alloc extraction of e-mail, given name, and surname.
    /// </summary>
    public static (string? Email, string? FirstName, string? LastName) GetEmailAndName(this User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        string? email = null;

        // One tight loop; avoids two FirstOrDefault LINQ calls and an extra list.
        if (user.Identities is not null)
        {
            foreach (ObjectIdentity id in user.Identities)
            {
                // a) Local/B2B/B2E accounts created with e-mail sign-in
                if (id.SignInType == "emailAddress")
                {
                    email = id.IssuerAssignedId;
                    break; // fastest exit – we’re done
                }

                // b) Social-federated identities (Google, Facebook, Apple…)
                if (email is null && id.SignInType == "federated")
                {
                    string? val = id.IssuerAssignedId;

                    if (val.HasContent() && val.IndexOf('@') >= 0)
                        email = val;
                }
            }
        }

        // c) Native Graph fields
        email ??= user.Mail ?? user.UserPrincipalName;

        //------------------------------------------------------------------//
        // 2️⃣  First & last names                                           //
        //------------------------------------------------------------------//

        string? first = user.GivenName;
        string? last = user.Surname;

        if (first.IsNullOrEmpty() || last.IsNullOrEmpty())
        {
            string? dn = user.DisplayName;

            if (dn.HasContent())
            {
                // Avoid Split allocation; find the first space only.
                int idx = dn.IndexOf(' ');

                if (idx < 0) // single-token name, e.g. “Madonna”
                {
                    first ??= dn;
                }
                else // everything before first space is First; the rest is Last
                {
                    if (first.IsNullOrEmpty())
                        first = dn[..idx]; // substring via range syntax; Span-friendly

                    if (last.IsNullOrEmpty())
                        last = dn[(idx + 1)..]; // no Trim needed – we know idx points at a space
                }
            }
        }

        // Final tidy-up (cheap if already null/non-whitespace)
        return (email?.Trim(), first?.Trim(), last?.Trim());
    }
}