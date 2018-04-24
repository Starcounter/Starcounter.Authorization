using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class CurrentUserProviderTests
    {
        private CurrentUserProvider<ScUserAuthenticationTicket, User> _sut;
        private User _returnedUser;
        private Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>();
            _sut = new CurrentUserProvider<ScUserAuthenticationTicket, User>(
                _authenticationTicketProviderMock.Object);
        }

        [Test]
        public void WhenCurrentTicketIsNullThenNullIsReturned()
        {
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns((ScUserAuthenticationTicket) null);

            Excercise();

            _returnedUser.Should().BeNull();
        }

        [Test]
        public void WhenCurrentTicketIsNotNullThenUserIsReturned()
        {
            var user = new User();
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket()
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