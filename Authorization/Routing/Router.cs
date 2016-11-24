using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Starcounter.Authorization.Routing
{
    public class Router
    {
        private readonly Func<RoutingInfo, Response> _pageCreator;
        private readonly List<IPageMiddleware> _middleware = new List<IPageMiddleware>();

        public Router(Func<RoutingInfo, Response> pageCreator)
        {
            _pageCreator = pageCreator;
        }

        public void HandleGet<T>()
        {
            HandleGet(typeof(T));
        }

        public void HandleGet(Type pageType)
        {
            var urlAttribute = pageType.GetCustomAttribute<UrlAttribute>();
            if (urlAttribute == null)
            {
                throw new Exception($"Type {pageType} has no Url attribute on it");
            }

            HandleGet(urlAttribute.Value, pageType);
        }

        public void HandleGet<T>(string url)
        {
            HandleGet(url, typeof(T));
        }

        public void HandleGet(string url, Type pageType)
        {
            var argumentsNo = Regex.Matches(url, @"\{\?\}").Count;
            switch (argumentsNo)
            {
                case 0:
                    Handle.GET(url, (Request request) => RunResponse(pageType, request));
                    break;
                case 1:
                    Handle.GET<string, Request>(url, (arg, request) => RunResponse(pageType, request, arg));
                    break;
                case 2:
                    Handle.GET<string, string, Request>(url,
                        (arg1, arg2, request) => RunResponse(pageType, request, arg1, arg2));
                    break;
                default:
                    throw new NotSupportedException("Not supported: more than 2 parameters in URL");
            }
        }

        public void AddMiddleware(IPageMiddleware middleware)
        {
            _middleware.Insert(0, middleware);
        }

        private Response RunResponse(Type pageType, Request request, params string[] arguments)
        {
            var routingInfo = new RoutingInfo { Request = request, SelectedPageType = pageType, Arguments = arguments };
            return RunWithMiddleware(
                routingInfo,
                _middleware.Concat(new [] { new TerminalMiddleware(() => _pageCreator(routingInfo)) }));
        }

        private Response RunWithMiddleware(RoutingInfo routingInfo, IEnumerable<IPageMiddleware> middlewares)
        {
            // todo enumerate only once
            // the last 'middleware' is actually just CreateAndInitPage which will always ignore the empty list of middleware it gets
            var pageMiddleware = middlewares.First();
            return pageMiddleware.Run(routingInfo, () => RunWithMiddleware(routingInfo, middlewares.Skip(1)));
        }

        private class TerminalMiddleware : IPageMiddleware
        {
            private readonly Func<Response> _creator;

            public TerminalMiddleware(Func<Response> creator)
            {
                _creator = creator;
            }

            public Response Run(RoutingInfo routingInfo, Func<Response> next)
            {
                return _creator();
            }
        }
    }
}