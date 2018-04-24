using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.SignIn
{
    public class SignInManagerTests
    {
        private SignInManager<ScUserAuthenticationTicket, User> _sut;
        private Mock<ICurrentSessionProvider> _sessionProviderMock;
        private User _user;
        private ScUserAuthenticationTicket _scUserAuthenticationTicket;
        private SignInOptions _options;
        private Mock<ISystemClock> _clockMock;
        private Base64ClaimsPrincipalSerializer _principalSerializer;
        private Mock<IUserClaimsGatherer> _claimsGathererMock;

        [SetUp]
        public void SetUp()
        {
            _sessionProviderMock = new Mock<ICurrentSessionProvider>();
            _options = new SignInOptions();
            _clockMock = new Mock<ISystemClock>();
            _principalSerializer = new Base64ClaimsPrincipalSerializer();
            _claimsGathererMock = new Mock<IUserClaimsGatherer>();
            _sut = new SignInManager<ScUserAuthenticationTicket, User>(
                _claimsGathererMock.Object,
                _principalSerializer, 
                _clockMock.Object,
                Options.Create(_options),
                _sessionProviderMock.Object,
                Mock.Of<ILogger<SignInManager<ScUserAuthenticationTicket, User>>>()
            );

            _user = new User()
            {
                AssociatedClaims = new IClaimDb[0],
                Groups = new UserGroup[0],
            };

            _scUserAuthenticationTicket = new ScUserAuthenticationTicket();

            _sessionProviderMock
                .SetupGet(provider => provider.CurrentSessionId)
                .Returns("sessionId");

            
        }

        [Test]
        public void SetsCurrentSessionIdInUserSession()
        {
            var sessionId = "sessionId";
            _sessionProviderMock
                .SetupGet(provider => provider.CurrentSessionId)
                .Returns(sessionId);

            Excercise();

            _scUserAuthenticationTicket.SessionId.Should().Be(sessionId);
        }

        [Test]
        public void SetsExpirationTimeAccordingToCurrentTimeAndOptions()
        {
            var fakeNow = DateTimeOffset.FromUnixTimeSeconds(123456);
            var ticketValidity = TimeSpan.FromHours(3);
            _options.NewTicketExpiration = ticketValidity;
            _clockMock.SetupGet(clock => clock.UtcNow).Returns(fakeNow);

            Excercise();

            _scUserAuthenticationTicket.ExpiresAt.Should().Be((fakeNow + ticketValidity).UtcDateTime);
        }

        [Test]
        public void ThrowsWhenCurrentSessionIsNull()
        {
            _sessionProviderMock
                .SetupGet(provider => provider.CurrentSessionId)
                .Returns((string) null);

            new Action(Excercise).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SetsUserInUserSession()
        {
            Excercise();

            _scUserAuthenticationTicket.User.Should().Be(_user);
        }

        [Test]
        public void SetsAuthenticationTypeInPrinicpalFromOptions()
        {
            var authenticationType = "authentication";
            _options.AuthenticationType = authenticationType;

            Excercise();

            _principalSerializer
                .Deserialize(_scUserAuthenticationTicket.PrincipalSerialized)
                .Identity.AuthenticationType
                .Should().Be(authenticationType);
        }

        [Test]
        public void AddsGatheredClaimsToPrincipal()
        {
            var stringClaim = new Claim("type1", "value1", "string", "issuer1");
            var intClaim = new Claim("type2", "2", "Int");
            _claimsGathererMock.Setup(gatherer => gatherer.Gather(_user))
                .Returns(new[] {stringClaim, intClaim});

            Excercise();

            _principalSerializer
                .Deserialize(_scUserAuthenticationTicket.PrincipalSerialized)
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

        private void Excercise()
        {
            _sut.SignIn(_user, _scUserAuthenticationTicket);
        }
    }
}