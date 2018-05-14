using System;
using Starcounter.Authorization.Authentication;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Middleware
{
    public class SecurityMiddleware : IPageMiddleware
    {
        private readonly PageSecurity.PageSecurity _pageSecurity;
        private readonly IAuthenticationUriProvider _authenticationUriProvider;

        public SecurityMiddleware(
            PageSecurity.PageSecurity pageSecurity,
            IAuthenticationUriProvider authenticationUriProvider
                )
        {
            _pageSecurity = pageSecurity;
            _authenticationUriProvider = authenticationUriProvider;
        }
        
        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            _pageSecurity.EnhanceClass(routingInfo.SelectedPageType);

            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Context).Result)
            {
                return _authenticationUriProvider.CreateUnauthenticatedRedirection(routingInfo.Request.Uri);
            }
            return next();
        }
    }
}