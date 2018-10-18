using System;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Middleware
{
    internal class EnsureSessionMiddleware: IPageMiddleware
    {
        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            Session.Ensure();
            return next();
        }
    }
}