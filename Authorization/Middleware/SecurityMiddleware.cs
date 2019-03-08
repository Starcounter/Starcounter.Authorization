using System;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Authentication;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Middleware
{
    internal class SecurityMiddleware<TUserAuthenticationTicket, TUser> : IPageMiddleware
        where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>
        where TUser : class, IMinimalUser
    {
        private readonly PageSecurity.PageSecurity _pageSecurity;
        private readonly Func<string, Json> _unauthenticatedResponseFactory;
        private readonly Func<Response> _unauthorizedResponseFactory;
        private readonly IAuthenticationTicketService<TUserAuthenticationTicket> _authenticationTicketService;

        public SecurityMiddleware(
            PageSecurity.PageSecurity pageSecurity,
            IOptions<SecurityMiddlewareOptions> options,
            IAuthenticationTicketService<TUserAuthenticationTicket> authenticationTicketService
        )
        {
            _pageSecurity = pageSecurity;
            _unauthenticatedResponseFactory = options.Value.UnauthenticatedResponseCreator;
            _unauthorizedResponseFactory = options.Value.UnauthorizedResponseCreator;
            _authenticationTicketService = authenticationTicketService;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Context).Result)
            {
                if (!routingInfo.Request.IsExternal)
                {
                    return null;
                }

                if (_authenticationTicketService.GetCurrentAuthenticationTicket()?.User == null)
                {
                    return _unauthenticatedResponseFactory(routingInfo.Request.Uri);
                }

                return _unauthorizedResponseFactory();
            }

            return next();
        }
    }
}