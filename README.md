[![](https://img.shields.io/nuget/v/soenneker.graph.extensions.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.extensions.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.extensions.users/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graph.extensions.users/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graph.extensions.users.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graph.extensions.users/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graph.extensions.users/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.graph.extensions.users/actions/workflows/codeql.yml)

# Soenneker.Graph.Extensions.Users

A collection of helpful GraphUser extension methods.

## Install

```bash
dotnet add package Soenneker.Graph.Extensions.Users
```

## Quick start

```csharp
using Soenneker.Graph.Extensions.Users;

User user = /* obtain from your application */;
var result = user.GetEmailAndName();
```

Extracts contact info in one pass with early-return shortcuts. **Requires** `$select` to include: `mail, userPrincipalName, otherMails, identities, givenName, surname, displayName`.

## What you get

- `GraphUsersExtension` — A collection of helpful GraphUser extension methods.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `GraphUsersExtension.GetEmailAndName(user)` | Extracts contact info in one pass with early-return shortcuts. **Requires** `$select` to include: `mail, userPrincipalName, otherMails, identities, givenName, surname, displayName`. | The resulting (string Email, string First Name, string Last Name). |
