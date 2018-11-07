using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication.AuthCookieService
{
    public class AuthCookieService_TryReattachToTicketWithTokenTests
    {
        private AuthCookieService<ScUserAuthenticationTicket> _sut;
        private bool _result;
        private List<string> _availableCookies;
        private Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>> _ticketServiceMock;
        private string _token;

        private const string CookieName = "scauthtoken";

        [SetUp]
        public void SetUp()
        {
            _ticketServiceMock = new Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>>();
            _sut = new AuthCookieService<ScUserAuthenticationTicket>(
                _ticketServiceMock.Object
                );
            _token = "token";
            _availableCookies = new List<string>() { $"{CookieName}={_token}" };
        }

        [Test]
        public void ReturnsTrueIfThereIsAlreadyATicket()
        {
            _ticketServiceMock
                .Setup(service => service.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket());
            Exercise();
            _result.Should().BeTrue();
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
            _ticketServiceMock
                .Setup(service => service.AttachToToken(token))
                .Returns(() => true);

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
        public void AttachesTicketToCurrentSession()
        {
            Exercise();

            _ticketServiceMock.Verify(service => service.AttachToToken(_token));
        }

        private void Exercise()
        {
            _result = _sut.TryReattachToTicketWithToken(_availableCookies);
        }
    }
}