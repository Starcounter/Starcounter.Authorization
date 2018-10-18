using System;
using Starcounter.Authorization.Authentication;
using Starcounter.Startup.Routing;
using Starcounter.Startup.Routing.Middleware;

namespace Starcounter.Authorization.Middleware
{
    internal class CookieSignInMiddleware<TAuthenticationTicket> : IPageMiddleware
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketProvider<TAuthenticationTicket> _authenticationTicketProvider;
        private readonly IAuthCookieService _authCookieService;

        public CookieSignInMiddleware(
            IAuthenticationTicketProvider<TAuthenticationTicket> authenticationTicketProvider,
            IAuthCookieService authCookieService
        )
        {
            _authenticationTicketProvider = authenticationTicketProvider;
            _authCookieService = authCookieService;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            // we could use Request.IsExternal, but it would be untestable
            if (UriHelper.IsPartialUri(routingInfo.Request.Uri))
            {
                // internal requests have no cookies
                _authenticationTicketProvider.EnsureTicket();
                return next();
            }

            if (_authenticationTicketProvider.GetCurrentAuthenticationTicket() != null)
            {
                return next();
            }
            if (_authCookieService.TryReattachToTicketWithToken(routingInfo.Request.Cookies))
            {
                return next();
            }

            _authenticationTicketProvider.EnsureTicket();
            var authCookie = _authCookieService.CreateAuthCookie();
            var response = next();
            response.Cookies.Add(authCookie);
            return response;
        }
    }
}