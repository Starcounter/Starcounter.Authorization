using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.SignIn
{
    public class SignInManagerTests
    {
        private SignInManager<ScUserAuthenticationTicket, User> _sut;
        private User _user;
        private ScUserAuthenticationTicket _scUserAuthenticationTicket;
        private Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>();
            _sut = new SignInManager<ScUserAuthenticationTicket, User>(
                _authenticationTicketProviderMock.Object, 
                Mock.Of<ILogger<SignInManager<ScUserAuthenticationTicket, User>>>()
            );

            _user = new User()
            {
                AssociatedClaims = new ClaimTemplate[0],
                MemberOf = new Group[0],
            };

            _scUserAuthenticationTicket = new ScUserAuthenticationTicket();
            _authenticationTicketProviderMock
                .Setup(provider => provider.EnsureTicket())
                .Returns(_scUserAuthenticationTicket);
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