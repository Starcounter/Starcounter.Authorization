using System;
using System.Linq;
using System.Linq.Expressions;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class SecurityMiddleware : IPageMiddleware
    {
        private readonly Func<RoutingInfo, Response> _unauthorizedHandler;
        private readonly PageSecurity.PageSecurity _pageSecurity;

        public SecurityMiddleware(
            IAuthorizationEnforcement authorizationEnforcement, 
            Func<RoutingInfo, Response> unauthorizedHandler,
            Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            _pageSecurity = new PageSecurity.PageSecurity(authorizationEnforcement, checkDeniedHandler);
            _unauthorizedHandler = unauthorizedHandler;
        }
        
        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            _pageSecurity.EnhanceClass(routingInfo.SelectedPageType);

//            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Context))
//            {
//                return _unauthorizedHandler(routingInfo);
//            }
            return next();
        }
    }
}