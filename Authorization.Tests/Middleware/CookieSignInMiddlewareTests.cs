using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Middleware;
using Starcounter.Authorization.Tests.TestModel;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Tests.Middleware
{
    public class CookieSignInMiddlewareTests
    {
        private CookieSignInMiddleware<ScUserAuthenticationTicket> _sut;
        private Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;
        private Mock<IAuthCookieService> _authCookieServiceMock;
        private Response _returnedResponse;
        private Func<Response> _next;
        private List<string> _cookies;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketService<ScUserAuthenticationTicket>>();
            _authCookieServiceMock = new Mock<IAuthCookieService>();
            _sut = new CookieSignInMiddleware<ScUserAuthenticationTicket>(
                _authenticationTicketProviderMock.Object,
                _authCookieServiceMock.Object
            );
            _authCookieServiceMock
                .Setup(service => service.CookieName)
                .Returns("cookie");
            _cookies = new List<string>();
            _next = () => new Response();
        }

        [Test]
        public void ReattachesTicketToCurrentSession()
        {
            Exercise();

            _authCookieServiceMock.Verify(service => service.TryReattachToTicketWithToken(_cookies));
        }

        [Test]
        public void DoesntReattachIfThereIsAlreadyAuthenticationTicket()
        {
            _authenticationTicketProviderMock
                .Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket());

            Exercise();

            VerifyNoReattachmentAttempts();
        }

        [Test]
        public void RunsTheRestOfMiddlewarePipeline()
        {
            var response = new Response();
            _next = () => response;

            Exercise();

            // using FluentAssertions' .Should().BeSameAs() requires Starcounter.BitsAndBytes to be loaded
            (_returnedResponse == response).Should().BeTrue();
        }

        private void Exercise()
        {
            _returnedResponse = _sut.Run(new RoutingInfo()
                {
                    Request = new Request()
                    {
                        Cookies = _cookies,
                        Uri = "/app/feature"
                    }
                },
                _next);
        }

        private void VerifyNoReattachmentAttempts()
        {
            _authCookieServiceMock.Verify(service => service.TryReattachToTicketWithToken(It.IsAny<IEnumerable<string>>()), Times.Never());
        }
    }
}