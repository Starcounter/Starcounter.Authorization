using System;
using Starcounter.Authorization.Authentication;
using Starcounter.Startup.Routing;
using Starcounter.Startup.Routing.Middleware;

namespace Starcounter.Authorization.Middleware
{
    internal class CookieSignInMiddleware<TAuthenticationTicket> : IPageMiddleware
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly IAuthCookieService _authCookieService;

        public CookieSignInMiddleware(
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            IAuthCookieService authCookieService
        )
        {
            _authenticationTicketService = authenticationTicketService;
            _authCookieService = authCookieService;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            // we could use Request.IsExternal, but it would be untestable
            if (UriHelper.IsPartialUri(routingInfo.Request.Uri))
            {
                // internal requests have no cookies
                _authenticationTicketService.EnsureTicket();
                return next();
            }

            if (_authenticationTicketService.GetCurrentAuthenticationTicket() != null)
            {
                return next();
            }
            if (_authCookieService.TryReattachToTicketWithToken(routingInfo.Request.Cookies))
            {
                return next();
            }

            _authenticationTicketService.EnsureTicket();
            var authCookie = _authCookieService.CreateAuthCookie();
            var response = next();
            Handle.AddOutgoingCookie(_authCookieService.CookieName, authCookie);
            return response;
        }
    }
}