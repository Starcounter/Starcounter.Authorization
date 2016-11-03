using System;
using System.Reflection;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class DbScopeMiddleware : IPageMiddleware
    {
        private readonly bool _defaultValue;

        public DbScopeMiddleware(bool defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            var attribute = routingInfo.SelectedPageType.GetCustomAttribute<UseDbScopeAttribute>();
            var useOrNot = attribute?.Value ?? _defaultValue;
            return useOrNot ? Db.Scope(next) : next();
        }
    }
}