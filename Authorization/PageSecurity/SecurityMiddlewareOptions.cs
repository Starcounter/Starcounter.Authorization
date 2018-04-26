using System;
using System.Linq.Expressions;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.PageSecurity
{
    public class SecurityMiddlewareOptions
    {
        public Func<RoutingInfo, Response> UnauthorizedAction { get; set; } = info => 403;

        public Func<Type, Expression, Expression, Expression> CheckDeniedHandler { get; set; } =
            PageSecurity.CreateThrowingDeniedHandler<Exception>();

        public SecurityMiddlewareOptions WithUnauthorizedAction(Func<RoutingInfo, Response> unauthorizedAction)
        {
            UnauthorizedAction = unauthorizedAction;
            return this;
        }

        public SecurityMiddlewareOptions WithCheckDeniedHandler(
            Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            CheckDeniedHandler = checkDeniedHandler;
            return this;
        }
    }
}