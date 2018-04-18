using System;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.PageSecurity;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class SecurityMiddleware : IPageMiddleware
    {
        private readonly Func<RoutingInfo, Response> _unauthorizedHandler;
        private readonly PageSecurity.PageSecurity _pageSecurity;

        public SecurityMiddleware(
            IOptions<SecurityMiddlewareOptions> options,
            PageSecurity.PageSecurity pageSecurity)
        {
            _pageSecurity = pageSecurity;
            _unauthorizedHandler = options.Value.UnauthorizedAction;
        }
        
        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            _pageSecurity.EnhanceClass(routingInfo.SelectedPageType);

            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Context).Result)
            {
                return _unauthorizedHandler(routingInfo);
            }
            return next();
        }
    }

}