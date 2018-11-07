﻿using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Settings;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class AuthenticationTicketServiceTests
    {
        private AuthenticationTicketService<ScUserAuthenticationTicket, User> _sut;
        private ScUserAuthenticationTicket _returnedAuthenticationTicket;
        private Mock<ICurrentSessionProvider> _sessionProviderMock;
        private Mock<ISystemClock> _clockMock;
        private Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>> _authenticationTicketRepositoryMock;
        private string _starcounterSessionId;
        private ScUserAuthenticationTicket _existingTicket;
        private DateTime _now;
        private AuthorizationOptions _options;

        [SetUp]
        public void SetUp()
        {
            _sessionProviderMock = new Mock<ICurrentSessionProvider>();
            _clockMock = new Mock<ISystemClock>();
            _options = new AuthorizationOptions();
            _authenticationTicketRepositoryMock = new Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>>();
            _sut = new AuthenticationTicketService<ScUserAuthenticationTicket, User>(
                Options.Create(_options), 
                _sessionProviderMock.Object,
                _clockMock.Object,
                _authenticationTicketRepositoryMock.Object,
                Mock.Of<ILogger<AuthenticationTicketService<ScUserAuthenticationTicket, User>>>(),
                Mock.Of<ISecureRandom>(),
                new FakeTransactionFactory());

            _starcounterSessionId = "sessionId";
            _now = DateTime.UtcNow;
            _existingTicket = new ScUserAuthenticationTicket()
            {
                ExpiresAt = _now.AddDays(1)
            };

            _sessionProviderMock.Setup(provider => provider.CurrentSessionId)
                .Returns(() => _starcounterSessionId);
            _authenticationTicketRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionId))
                .Returns(() => _existingTicket);
            _authenticationTicketRepositoryMock.Setup(repository => repository.Create())
                .Returns(() => new ScUserAuthenticationTicket());
            _clockMock.Setup(clock => clock.UtcNow)
                .Returns(() => _now);
        }

        [Test]
        public void Current_WhenCurrentSessionIsNullThenNullIsReturned()
        {
            _starcounterSessionId = null;

            ExerciseCurrent();

            _returnedAuthenticationTicket.Should().BeNull();
        }

        [Test]
        public void Current_WhenCurrentSessionHasNoCorrespondingUserTicketThenNullIsReturned()
        {
            _authenticationTicketRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionId))
                .Returns((ScUserAuthenticationTicket) null);

            ExerciseCurrent();

            _returnedAuthenticationTicket.Should().BeNull();
        }

        [Test]
        public void Current_WhenCurrentTicketHasExpiredThenNullIsReturnedAndTheTicketIsDeleted()
        {
            _existingTicket.ExpiresAt = _now.AddDays(-1);

            ExerciseCurrent();

            _returnedAuthenticationTicket.Should().BeNull();
            _authenticationTicketRepositoryMock.Verify(repository => repository.Delete(_existingTicket));
        }

        [Test]
        public void Current_WhenEverythingChecksOutThenTheTicketFromRepositoryIsReturned()
        {

            ExerciseCurrent();

            _returnedAuthenticationTicket.Should().BeSameAs(_existingTicket);
        }

        [Test]
        public void Current_ResetsExpirationTime()
        {
            var fakeNow = DateTimeOffset.FromUnixTimeSeconds(123456);
            var ticketValidity = TimeSpan.FromHours(3);
            _options.AuthenticatedTicketExpiration = ticketValidity;
            _clockMock.SetupGet(clock => clock.UtcNow).Returns(fakeNow);

            ExerciseCurrent();

            _returnedAuthenticationTicket.ExpiresAt.Should().Be((fakeNow + ticketValidity).UtcDateTime);
        }

        [Test]
        public void Create_SetsCurrentSessionIdInUserSession()
        {
            _existingTicket = null;

            ExerciseCreate();

            _returnedAuthenticationTicket.SessionId.Should().Be(_starcounterSessionId);
        }

        [Test]
        public void Create_SetsExpirationTimeAccordingToCurrentTimeAndOptions()
        {
            _existingTicket = null;
            var fakeNow = DateTimeOffset.FromUnixTimeSeconds(123456);
            var ticketValidity = TimeSpan.FromHours(3);
            _options.AnonymousTicketExpiration = ticketValidity;
            _clockMock.SetupGet(clock => clock.UtcNow).Returns(fakeNow);

            ExerciseCreate();

            _returnedAuthenticationTicket.ExpiresAt.Should().Be((fakeNow + ticketValidity).UtcDateTime);
        }

        [Test]
        public void Create_ThrowsWhenCurrentSessionIsNull()
        {
            _sessionProviderMock
                .SetupGet(provider => provider.CurrentSessionId)
                .Returns((string)null);

            new Action(ExerciseCreate).Should().Throw<InvalidOperationException>();
        }

        private void ExerciseCurrent()
        {
            _returnedAuthenticationTicket = _sut.GetCurrentAuthenticationTicket();
        }

        private void ExerciseCreate()
        {
            _returnedAuthenticationTicket = _sut.Create();
        }
    }
}