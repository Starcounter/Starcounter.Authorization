using System;

namespace Starcounter.Authorization.Routing
{
    public interface IPageMiddleware
    {
        Response Run(RoutingInfo routingInfo, Func<Response> next);
    }
}