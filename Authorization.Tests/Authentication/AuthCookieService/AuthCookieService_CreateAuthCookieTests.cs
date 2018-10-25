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
        private Mock<ISecureRandom> _secureRandomMock;
        private ScUserAuthenticationTicket _ticket;
        private string _randomToken;
        private string _returnedCookieValue;
        private string _returnedCookieAttributes;
        private string _returnedCookieString;
        private Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;

        [SetUp]
        public void SetUp()
        {
            _secureRandomMock = new Mock<ISecureRandom>();
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>>();
            _sut = new AuthCookieService<ScUserAuthenticationTicket>(
                Mock.Of<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>>(),
                Mock.Of<ICurrentSessionProvider>(),
                _secureRandomMock.Object,
                _authenticationTicketProviderMock.Object,
                new FakeTransactionFactory());
            _ticket = new ScUserAuthenticationTicket();
            _authenticationTicketProviderMock
                .Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(() => _ticket);
            _randomToken = "randomString";
            _secureRandomMock
                .Setup(random => random.GenerateRandomHexString(It.IsAny<int>()))
                .Returns(_randomToken);
        }

        [Test]
        public void PutsRandomStringInTicket()
        {
            _ticket = new ScUserAuthenticationTicket();

            Exercise();

            _ticket.PersistenceToken.Should().Be(_randomToken);
        }

        [Test]
        public void ReturnsNullIfThereIsNoAuthenticationTicket()
        {
            _ticket = null;

            Exercise();

            _returnedCookieValue.Should().Be(_randomToken);
        }

        [Test]
        public void ReturnsTheRandomStringAsCookie()
        {
            Exercise();

            _returnedCookieValue.Should().Be(_randomToken);
        }

        [Test]
        public void ReturnsHttpOnlyCookieWithGlobalPath()
        {

            Exercise();

            _returnedCookieAttributes.Should().Be("HttpOnly;Path=/");
        }

        private void Exercise()
        {
            _returnedCookieString = _sut.CreateAuthCookie();
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