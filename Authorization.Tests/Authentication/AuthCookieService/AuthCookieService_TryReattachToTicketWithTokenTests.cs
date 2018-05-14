using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Tests.TestModel;
using static Starcounter.Authorization.Authentication.AuthCookieService<Starcounter.Authorization.Tests.TestModel.ScUserAuthenticationTicket>;

namespace Starcounter.Authorization.Tests.Authentication.AuthCookieService
{
    public class AuthCookieService_TryReattachToTicketWithTokenTests
    {
        private AuthCookieService<ScUserAuthenticationTicket> _sut;
        private Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>> _ticketRepositoryMock;
        private Mock<ICurrentSessionProvider> _currentSessionProviderMock;
        private bool _result;
        private List<string> _availableCookies;
        private ScUserAuthenticationTicket _ticket;
        private string _sessionId;

        [SetUp]
        public void SetUp()
        {
            _ticketRepositoryMock = new Mock<IScAuthenticationTicketRepository<ScUserAuthenticationTicket>>();
            _currentSessionProviderMock = new Mock<ICurrentSessionProvider>();
            _sut = new AuthCookieService<ScUserAuthenticationTicket>(
                _ticketRepositoryMock.Object,
                _currentSessionProviderMock.Object,
                Mock.Of<ISecureRandom>(),
                Mock.Of<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>(),
                new FakeTransactionFactory());
            var token = "token";
            _availableCookies = new List<string>() { $"{CookieName}={token}" };
            _ticket = new ScUserAuthenticationTicket();
            _sessionId = "sessionId";
            _ticketRepositoryMock
                .Setup(repository => repository.FindByPersistenceToken(token))
                .Returns(() => _ticket);
            _currentSessionProviderMock.Setup(provider => provider.CurrentSessionId)
                .Returns(() => _sessionId);
        }

        [Test]
        public void ReturnsFalseIfNoTicketIsFoundForToken()
        {
            _ticket = null;

            Exercise();

            _result.Should().BeFalse();
        }

        [TestCaseSource(nameof(NonMatchingCookiesSets))]
        public void ReturnsFalseIfNoCookiesMatchTheName(List<string> cookies)
        {
            _availableCookies = cookies;

            Exercise();

            _result.Should().BeFalse();
        }

        public static IEnumerable NonMatchingCookiesSets()
        {
            yield return new TestCaseData(new List<string>()).SetName("<empty>");
            yield return new TestCaseData(new List<string>() { "name" }).SetName("{ \"name\" }");
            yield return new TestCaseData(new List<string>() { "name=value" }).SetName("{ \"name=value\" }");
            yield return new TestCaseData(new List<string>() { "name=value", "name2=value" }).SetName("{ \"name=value\", \"name2=value\" }");
            yield return new TestCaseData(new List<string>() { "name=value;attr=value" }).SetName("{ \"name=value;attr=value\" }");
        }

        [TestCaseSource(nameof(CookiesWithVariousFormats))]
        public bool HandlesVariousFormats(string cookie, string token)
        {
            _availableCookies = new List<string>(){cookie};
            _ticketRepositoryMock
                .Setup(repository => repository.FindByPersistenceToken(token))
                .Returns(() => _ticket);

            Exercise();

            return _result;
        }

        public static IEnumerable CookiesWithVariousFormats()
        {
            var value = "value";
            yield return new TestCaseData(CookieName, value).Returns(false);
            yield return new TestCaseData($"{CookieName}=", value).Returns(false);
            yield return new TestCaseData($"{CookieName}={value}", value).Returns(true);
            yield return new TestCaseData($"{CookieName}={value};Attribute", value).Returns(true);
            yield return new TestCaseData($"{CookieName}={value};Attribute=value", value).Returns(true);
            yield return new TestCaseData($"{CookieName}={value};Attribute;Attribute2", value).Returns(true);
            yield return new TestCaseData($"{CookieName}={value};Attribute;Attribute2=value", value).Returns(true);
        }

        [Test]
        public void ReturnsTrue()
        {
            Exercise();

            _result.Should().BeTrue();
        }

        [Test]
        public void AttachesTicketToCurrentSession()
        {
            Exercise();

            _ticket.SessionId.Should().Be(_sessionId);
        }

        private void Exercise()
        {
            _result = _sut.TryReattachToTicketWithToken(_availableCookies);
        }
    }
}