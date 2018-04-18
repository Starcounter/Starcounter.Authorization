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
    public class CurrentUserProviderTests
    {
        private CurrentUserProvider<UserSession, User> _sut;
        private User _returnedUser;
        private Mock<ICurrentSessionRetriever<UserSession>> _sessionRetrieverMock;

        [SetUp]
        public void SetUp()
        {
            _sessionRetrieverMock = new Mock<ICurrentSessionRetriever<UserSession>>();
            _sut = new CurrentUserProvider<UserSession, User>(
                _sessionRetrieverMock.Object);
        }

        [Test]
        public void WhenCurrentSessionIsNullThenNullIsReturned()
        {
            _sessionRetrieverMock.Setup(retriever => retriever.GetCurrentSession())
                .Returns((UserSession) null);

            Excercise();

            _returnedUser.Should().BeNull();
        }

        [Test]
        public void WhenCurrentSessionIsNotNullThenUserIsReturned()
        {
            var user = new User();
            _sessionRetrieverMock.Setup(retriever => retriever.GetCurrentSession())
                .Returns(new UserSession()
                {
                    User = user
                });

            Excercise();

            _returnedUser.Should().BeSameAs(user);
        }

        private void Excercise()
        {
            _returnedUser = _sut.GetCurrentUser();
        }
    }
}