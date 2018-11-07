using System.Text.RegularExpressions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication.AuthCookieService
{
    public class AuthCookieService_CreateAuthCookieTests
    {
        private AuthCookieService<ScUserAuthenticationTicket> _sut;
        private ScUserAuthenticationTicket _ticket;
        private string _returnedCookieValue;
        private string _returnedCookieAttributes;
        private string _returnedCookieString;
        private Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>>();
            _sut = new AuthCookieService<ScUserAuthenticationTicket>(
                _authenticationTicketProviderMock.Object
                );
            _ticket = new ScUserAuthenticationTicket
            {
                PersistenceToken = "randomString"
            };
        }

        [Test]
        public void ReturnsTheRandomStringAsCookie()
        {
            Exercise();

            _returnedCookieValue.Should().Be(_ticket.PersistenceToken);
        }

        [Test]
        public void ReturnsHttpOnlyCookieWithGlobalPath()
        {

            Exercise();

            _returnedCookieAttributes.Should().Be("HttpOnly;Path=/");
        }

        private void Exercise()
        {
            _returnedCookieString = _sut.CreateAuthCookie(_ticket);
            if (_returnedCookieString == null)
            {
                return;
            }

            var matches = Regex.Match(_returnedCookieString, "^(?<value>[a-zA-Z0-9]+);(?<other>.*)");
            _returnedCookieValue = matches.Groups["value"].Value;
            _returnedCookieAttributes = matches.Groups["other"].Value;
        }
    }
}