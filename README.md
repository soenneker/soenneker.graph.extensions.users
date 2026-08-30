[![](https://img.shields.io/nuget/v/soenneker.graph.extensions.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.extensions.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.extensions.users/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.extensions.users/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.extensions.users/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.extensions.users/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graph.extensions.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.extensions.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.extensions.users/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.graph.extensions.users/actions/workflows/codeql.yml)

# Soenneker.Graph.Extensions.Users

Extracts a practical email, first name, and last name from a Microsoft Graph `User` whose identity fields may be incomplete.

## Install

```bash
dotnet add package Soenneker.Graph.Extensions.Users
```

## Usage

```csharp
using Soenneker.Graph.Extensions.Users;

User? user = await graph.Users[userId].GetAsync(config =>
{
    config.QueryParameters.Select =
    [
        "mail",
        "userPrincipalName",
        "otherMails",
        "identities",
        "givenName",
        "surname",
        "displayName"
    ];
}, cancellationToken);

if (user is not null)
{
    (string? email, string? firstName, string? lastName) =
        user.GetEmailAndName();
}
```

When loading a user from Graph, select the fields shown above. The extension can only consider properties present on the supplied model.

## Selection rules

- Email preference is `Mail`, then `UserPrincipalName`, then the first nonblank `OtherMails` value.
- If those are absent, an `emailAddress` identity wins; a federated identity containing `@` is a fallback.
- Existing `GivenName` and `Surname` values win.
- Missing names are inferred from `DisplayName`: a single token becomes the first name; for multiple tokens, the first and last tokens are used.
- Returned values are trimmed. A value remains `null` when no usable source exists.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `user.GetEmailAndName()` | Applies the email and name fallback rules above. | A nullable `(Email, FirstName, LastName)` tuple. |
