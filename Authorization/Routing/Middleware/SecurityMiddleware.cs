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

            if (!_pageSecurity.CheckClass(routingInfo.SelectedPageType, routingInfo.Arguments.Select(DbObjectFromBase64Id).ToArray()))
            {
                return _unauthorizedHandler(routingInfo);
            }
            return next();
        }

        private static object DbObjectFromBase64Id(string id)
        {
            ulong objectId;
            try
            {
                objectId = DbHelper.Base64DecodeObjectID(id);
            }
            catch (ArgumentException)
            {
                return null; // in case a "random" string is supplied, it's leniently converted to int and FromID returns null
            }
            return DbHelper.FromID(objectId);
        }
    }
}