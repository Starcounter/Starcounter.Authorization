using System;
using System.Linq;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Middleware
{
    public class CookieSignInMiddleware<TAuthenticationTicket> : IPageMiddleware where TAuthenticationTicket : class, IScAuthenticationTicket
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
            if (_authenticationTicketProvider.GetCurrentAuthenticationTicket() != null)
            {
                return next();
            }

            _authCookieService.TryReattachToTicketWithToken(routingInfo.Request.Cookies);
            return next();
        }
    }
}