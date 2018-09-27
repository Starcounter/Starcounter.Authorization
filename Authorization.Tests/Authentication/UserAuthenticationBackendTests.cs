using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class UserAuthenticationBackendTests
    {
        private IAuthenticationBackend _sut;
        private Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;
        private ClaimsPrincipal _returnedPrincipal;
        private Mock<IUserClaimsGatherer> _claimsGathererMock;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>();
            _claimsGathererMock = new Mock<IUserClaimsGatherer>();
            _sut = new UserAuthenticationBackend<ScUserAuthenticationTicket, User>(
                _authenticationTicketProviderMock.Object,
                _claimsGathererMock.Object
            );
        }

        [Test]
        public void WhenAuthenticationTicketIsNullThenAnonymousPrincipalIsReturned()
        {
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns((ScUserAuthenticationTicket) null);

            Excercise();

            _returnedPrincipal.Identities.Should().BeEmpty();
            _returnedPrincipal.Claims.Should().BeEmpty();
        }

        [Test]
        public void WhenAuthenticationTicketIsNotNullThenPrincipalWithGatheredClaimsIsReturned()
        {
            var user = new User();
            _authenticationTicketProviderMock
                .Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket()
                {
                    User = user
                });
            var stringClaim = new Claim("type1", "value1", "string", "issuer1");
            var intClaim = new Claim("type2", "2", "Int");
            _claimsGathererMock.Setup(gatherer => gatherer.Gather(user))
                .Returns(new[] { stringClaim, intClaim });

            Excercise();

            _returnedPrincipal
                .Claims
                .Should().HaveCount(2)
                .And.Contain(claim =>
                    claim.Value == stringClaim.Value && claim.Type == stringClaim.Type
                                                     && claim.ValueType == stringClaim.ValueType
                                                     && claim.Issuer == stringClaim.Issuer)
                .And.Contain(claim =>
                    claim.Value == intClaim.Value && claim.Type == intClaim.Type
                                                  && claim.ValueType == intClaim.ValueType
                                                  && claim.Issuer == intClaim.Issuer);
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