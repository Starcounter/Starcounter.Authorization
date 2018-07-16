using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class AuthenticationBackendTests
    {
        private IAuthenticationBackend _sut;
        private Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;
        private ClaimsPrincipal _returnedPrincipal;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>();
            _sut = new AuthenticationBackend<ScUserAuthenticationTicket>(
                _authenticationTicketProviderMock.Object
            );
        }

        [Test]
        public void WhenAuthenticationTicketIsNullThenAnonymousPrincipalIsReturned()
        {
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns((ScUserAuthenticationTicket)null);

            Excercise();

            _returnedPrincipal.Identities.Should().BeEmpty();
            _returnedPrincipal.Claims.Should().BeEmpty();
        }

        [Test]
        public void WhenAuthenticationTicketIsNotNullThenAuthenticatedPrincipalIsReturned()
        {
            _authenticationTicketProviderMock
                .Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket());

            Excercise();

            // derived from Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement
            _returnedPrincipal?.Identities?.Any(i => i.IsAuthenticated).Should().BeTrue();
        }

        private void Excercise()
        {
            _returnedPrincipal = _sut.GetCurrentPrincipal();
        }
    }
}