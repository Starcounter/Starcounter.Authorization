using System;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class SecurityMiddleware : IPageMiddleware
    {
        private readonly IAuthorizationEnforcement _authorizationEnforcement;
        private readonly Func<RoutingInfo, Response> _unauthorizedHandler;

        public SecurityMiddleware(IAuthorizationEnforcement authorizationEnforcement, Func<RoutingInfo, Response> unauthorizedHandler)
        {
            _authorizationEnforcement = authorizationEnforcement;
            _unauthorizedHandler = unauthorizedHandler;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            if(!PageSecurity.PageSecurity.CheckClass(routingInfo.SelectedPageType, _authorizationEnforcement))
            {
                return _unauthorizedHandler(routingInfo);
            }
            return next();
        }
    }
}