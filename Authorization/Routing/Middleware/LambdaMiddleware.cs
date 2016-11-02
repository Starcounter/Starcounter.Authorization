using System;

namespace Starcounter.Authorization.Routing.Middleware
{
    public class LambdaMiddleware : IPageMiddleware
    {
        private readonly Func<RoutingInfo, Func<Response>, Response> _lambda;

        public LambdaMiddleware(Func<RoutingInfo, Func<Response>, Response> lambda)
        {
            _lambda = lambda;
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            return _lambda(routingInfo, next);
        }
    }
}