using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class AuthenticationTicketProviderTests
    {
        private AuthenticationTicketProvider<ScUserAuthenticationTicket> _sut;
        private ScUserAuthenticationTicket _returnedAuthenticationTicket;
        private Mock<ICurrentSessionProvider> _sessionProviderMock;
        private Mock<ISystemClock> _clockMock;
        private Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>> _authenticationTicketRepositoryMock;
        private string _starcounterSessionid;
        private ScUserAuthenticationTicket _scUserAuthenticationTicket;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _sessionProviderMock = new Mock<ICurrentSessionProvider>();
            _clockMock = new Mock<ISystemClock>();
            _authenticationTicketRepositoryMock = new Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>>();
            _sut = new AuthenticationTicketProvider<ScUserAuthenticationTicket>(
                _sessionProviderMock.Object,
                _clockMock.Object,
                _authenticationTicketRepositoryMock.Object,
                Mock.Of<ILogger<AuthenticationTicketProvider<ScUserAuthenticationTicket>>>(),
                new FakeTransactionFactory());

            _starcounterSessionid = "sessionId";
            _now = DateTime.UtcNow;
            _scUserAuthenticationTicket = new ScUserAuthenticationTicket()
            {
                ExpiresAt = _now.AddDays(1)
            };

            _sessionProviderMock.Setup(provider => provider.CurrentSessionId)
                .Returns(() => _starcounterSessionid);
            _authenticationTicketRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionid))
                .Returns(() => _scUserAuthenticationTicket);
            _clockMock.Setup(clock => clock.UtcNow)
                .Returns(() => _now);
        }

        [Test]
        public void WhenCurrentSessionIsNullThenNullIsReturned()
        {
            _starcounterSessionid = null;

            Excercise();

            _returnedAuthenticationTicket.Should().BeNull();
        }

        [Test]
        public void WhenCurrentSessionHasNoCorrespondingUserTicketThenNullIsReturned()
        {
            _authenticationTicketRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionid))
                .Returns((ScUserAuthenticationTicket) null);

            Excercise();

            _returnedAuthenticationTicket.Should().BeNull();
        }

        [Test]
        public void WhenCurrentTicketHasExpiredThenNullIsReturnedAndTheTicketIsDeleted()
        {
            _scUserAuthenticationTicket.ExpiresAt = _now.AddDays(-1);

            Excercise();

            _returnedAuthenticationTicket.Should().BeNull();
            _authenticationTicketRepositoryMock.Verify(repository => repository.Delete(_scUserAuthenticationTicket));
        }

        [Test]
        public void WhenEverythingChecksOutThenTheTicketFromRepositoryIsReturned()
        {

            Excercise();

            _returnedAuthenticationTicket.Should().BeSameAs(_scUserAuthenticationTicket);
        }



        private void Excercise()
        {
            _returnedAuthenticationTicket = _sut.GetCurrentAuthenticationTicket();
        }
    }
}