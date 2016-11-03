using System;
using System.Linq;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class SecurityMiddleware : IPageMiddleware
    {
        private readonly Func<RoutingInfo, Response> _unauthorizedHandler;
        private readonly PageSecurity.PageSecurity _pageSecurity;

        public SecurityMiddleware(IAuthorizationEnforcement authorizationEnforcement, Func<RoutingInfo, Response> unauthorizedHandler)
        {
            _pageSecurity = new PageSecurity.PageSecurity(authorizationEnforcement);
            _unauthorizedHandler = unauthorizedHandler;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            _pageSecurity.EnhanceClass(routingInfo.SelectedPageType);

            object data = null;
            if (routingInfo.Arguments.Length == 1)
            {
                var dataType = routingInfo.SelectedPageType.GetInterface($"{nameof(IBound<int>)}`1")?.GetGenericArguments().First();
                if (dataType != null)
                {
                    data = DbHelper.FromID(DbHelper.Base64DecodeObjectID(routingInfo.Arguments[0]));
                }
            }
            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, data))
            {
                return _unauthorizedHandler(routingInfo);
            }
            return next();
        }
    }
}