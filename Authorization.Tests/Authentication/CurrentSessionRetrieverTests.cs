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
    public class CurrentSessionRetrieverTests
    {
        private CurrentSessionRetriever<UserSession> _sut;
        private UserSession _returnedSession;
        private Mock<ICurrentSessionProvider> _sessionProviderMock;
        private Mock<ISystemClock> _clockMock;
        private Mock<ISessionRepository<UserSession>> _sessionRepositoryMock;
        private string _starcounterSessionid;
        private UserSession _userSession;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _sessionProviderMock = new Mock<ICurrentSessionProvider>();
            _clockMock = new Mock<ISystemClock>();
            _sessionRepositoryMock = new Mock<ISessionRepository<UserSession>>();
            _sut = new CurrentSessionRetriever<UserSession>(
                _sessionProviderMock.Object,
                _clockMock.Object,
                _sessionRepositoryMock.Object,
                Mock.Of<ILogger<CurrentSessionRetriever<UserSession>>>());

            _starcounterSessionid = "sessionId";
            _now = DateTime.UtcNow;
            _userSession = new UserSession()
            {
                ExpiresAt = _now.AddDays(1)
            };

            _sessionProviderMock.Setup(provider => provider.CurrentSessionId)
                .Returns(() => _starcounterSessionid);
            _sessionRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionid))
                .Returns(() => _userSession);
            _clockMock.Setup(clock => clock.UtcNow)
                .Returns(() => _now);
        }

        [Test]
        public void WhenCurrentSessionIsNullThenNullIsReturned()
        {
            _starcounterSessionid = null;

            Excercise();

            _returnedSession.Should().BeNull();
        }

        [Test]
        public void WhenCurrentSessionHasNoCorrespondingUserSessionThenNullIsReturned()
        {
            _sessionRepositoryMock.Setup(repository => repository.FindBySessionId(_starcounterSessionid))
                .Returns((UserSession) null);

            Excercise();

            _returnedSession.Should().BeNull();
        }

        [Test]
        public void WhenCurrentSessionHasExpiredThenNullIsReturnedAndTheSessionIsDeleted()
        {
            _userSession.ExpiresAt = _now.AddDays(-1);

            Excercise();

            _returnedSession.Should().BeNull();
            _sessionRepositoryMock.Verify(repository => repository.Delete(_userSession));
        }

        [Test]
        public void WhenEverythingChecksOutThenTheSessionFromRepositoryIsReturned()
        {

            Excercise();

            _returnedSession.Should().BeSameAs(_userSession);
        }



        private void Excercise()
        {
            _returnedSession = _sut.GetCurrentSession();
        }
    }
}