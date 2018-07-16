using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Model;
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
        private Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>> _authenticationTicketRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _sessionProviderMock = new Mock<ICurrentSessionProvider>();
            _options = new SignInOptions();
            _clockMock = new Mock<ISystemClock>();
            _authenticationTicketRepositoryMock = new Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>>();
            _sut = new SignInManager<ScUserAuthenticationTicket, User>(_clockMock.Object,
                Options.Create(_options),
                _sessionProviderMock.Object,
                Mock.Of<ILogger<SignInManager<ScUserAuthenticationTicket, User>>>(),
                _authenticationTicketRepositoryMock.Object
            );

            _user = new User()
            {
                AssociatedClaims = new ClaimTemplate[0],
                MemberOf = new Group[0],
            };

            _scUserAuthenticationTicket = new ScUserAuthenticationTicket();
            _authenticationTicketRepositoryMock
                .Setup(repository => repository.Create())
                .Returns(_scUserAuthenticationTicket);

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

        private void Excercise()
        {
            _sut.SignIn(_user);
        }
    }
}