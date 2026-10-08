using System.Threading.Tasks;
using Microsoft.Graph.Models;
using Soenneker.Tests.Unit;
using System.Threading;

namespace Soenneker.Graph.Extensions.Users.Tests;

public sealed class GraphUsersExtensionTests : UnitTest
{
    [Test]
    public void Default()
    {

    }

    [Test]
    public async ValueTask Whitespace_values_do_not_hide_fallbacks(CancellationToken cancellationToken)
    {
        var user = new User
        {
            Mail = "  ",
            UserPrincipalName = "person@example.com",
            GivenName = " ",
            Surname = null,
            DisplayName = "Ada Lovelace"
        };

        (string? email, string? firstName, string? lastName) = user.GetEmailAndName();

        await Assert.That(email).IsEqualTo("person@example.com");
        await Assert.That(firstName).IsEqualTo("Ada");
        await Assert.That(lastName).IsEqualTo("Lovelace");
    }
}
