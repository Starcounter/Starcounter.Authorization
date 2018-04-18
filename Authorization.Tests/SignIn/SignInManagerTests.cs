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
        private SignInManager<UserSession, User> _sut;
        private Mock<ICurrentSessionProvider> _sessionProviderMock;
        private User _user;
        private UserSession _userSession;
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
            _sut = new SignInManager<UserSession, User>(
                _claimsGathererMock.Object,
                _principalSerializer, 
                _clockMock.Object,
                Options.Create(_options),
                _sessionProviderMock.Object,
                Mock.Of<ILogger<SignInManager<UserSession, User>>>()
            );

            _user = new User()
            {
                AssociatedClaims = new IClaimDb[0],
                Groups = new UserGroup[0],
            };

            _userSession = new UserSession();

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

            _userSession.SessionId.Should().Be(sessionId);
        }

        [Test]
        public void SetsExpirationTimeAccordingToCurrentTimeAndOptions()
        {
            var fakeNow = DateTimeOffset.FromUnixTimeSeconds(123456);
            var ticketValidity = TimeSpan.FromHours(3);
            _options.NewTicketExpiration = ticketValidity;
            _clockMock.SetupGet(clock => clock.UtcNow).Returns(fakeNow);

            Excercise();

            _userSession.ExpiresAt.Should().Be((fakeNow + ticketValidity).UtcDateTime);
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

            _userSession.User.Should().Be(_user);
        }

        [Test]
        public void SetsAuthenticationTypeInPrinicpalFromOptions()
        {
            var authenticationType = "authentication";
            _options.AuthenticationType = authenticationType;

            Excercise();

            _principalSerializer
                .Deserialize(_userSession.PrincipalSerialized)
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
                .Deserialize(_userSession.PrincipalSerialized)
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
            _sut.SignIn(_user, _userSession);
        }
    }
}