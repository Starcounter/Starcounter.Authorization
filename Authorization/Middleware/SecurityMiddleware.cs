using System;
using Microsoft.Extensions.Options;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Middleware
{
    internal class SecurityMiddleware : IPageMiddleware
    {
        private readonly PageSecurity.PageSecurity _pageSecurity;
        private readonly Func<string, Json> _unauthenticatedResponseFactory;

        public SecurityMiddleware(
            PageSecurity.PageSecurity pageSecurity,
            IOptions<SecurityMiddlewareOptions> options
                )
        {
            _pageSecurity = pageSecurity;
            _unauthenticatedResponseFactory = options.Value.UnauthenticatedResponseCreator;
        }
        
        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            _pageSecurity.EnhanceClass(routingInfo.SelectedPageType);

            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Context).Result)
            {
                return _unauthenticatedResponseFactory(routingInfo.Request.Uri);
            }
            return next();
        }
    }
}