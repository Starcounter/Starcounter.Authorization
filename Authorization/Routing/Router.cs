using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Starcounter.Authorization.Routing
{
    public interface IPageCreator
    {
        Response Create(RoutingInfo routingInfo);
    }

    /// <summary>
    /// Creates a router that will create pages using default constructor and call <see cref="PageContextSupport.HandleContext"/>
    /// Works only with pages that are <see cref="IResource"/> - this category includes <see cref="Json"/>, so covers most cases
    /// </summary>
    /// <param name="routingInfo"></param>
    /// <returns></returns>
    public class DefaultPageCreator : IPageCreator
    {
        public Response Create(RoutingInfo routingInfo)
        {
            var page = Activator.CreateInstance(routingInfo.SelectedPageType);
            if (page is IInitPage initPage)
            {
                initPage.Init();
            }
            PageContextSupport.HandleContext(page, routingInfo.Context);
            return new Response() { Resource = (IResource)page };
        }
    }

    public class Router
    {
        private readonly IPageCreator _pageCreator;
        private readonly List<IPageMiddleware> _middleware;

        public Router(IPageCreator pageCreator, IEnumerable<IPageMiddleware> middlewares)
        {
            _pageCreator = pageCreator;
            _middleware = middlewares.ToList();
        }

        public void HandleGet<T>(HandlerOptions handlerOptions = null)
        {
            HandleGet(typeof(T), handlerOptions);
        }

        public void HandleGet(Type pageType, HandlerOptions handlerOptions = null)
        {
            var urlAttribute = pageType.GetCustomAttribute<UrlAttribute>();
            if (urlAttribute == null)
            {
                throw new Exception($"Type {pageType} has no Url attribute on it");
            }

            HandleGet(urlAttribute.Value, pageType, handlerOptions);
        }

        public void HandleGet<T>(string url, HandlerOptions handlerOptions = null)
        {
            HandleGet(url, typeof(T), handlerOptions);
        }

        public void HandleGet(string url, Type pageType, HandlerOptions handlerOptions = null)
        {
            var argumentsNo = Regex.Matches(url, @"\{\?\}").Count;
            switch (argumentsNo)
            {
                case 0:
                    Handle.GET(url,
                        (Request request) => RunResponse(pageType, request),
                        handlerOptions);
                    break;
                case 1:
                    Handle.GET<string, Request>(url,
                        (arg, request) => RunResponse(pageType, request, arg),
                        handlerOptions);
                    break;
                case 2:
                    Handle.GET<string, string, Request>(url,
                        (arg1, arg2, request) => RunResponse(pageType, request, arg1, arg2),
                        handlerOptions);
                    break;
                case 3:
                    Handle.GET<string, string, string, Request>(url,
                        (arg1, arg2, arg3, request) => RunResponse(pageType, request, arg1, arg2, arg3),
                        handlerOptions);
                    break;
                case 4:
                    Handle.GET<string, string, string, string, Request>(url,
                        (arg1, arg2, arg3, arg4, request) => RunResponse(pageType, request, arg1, arg2, arg3, arg4),
                        handlerOptions);
                    break;
                default:
                    throw new NotSupportedException("Not supported: more than 4 parameters in URL");
            }
        }

        private Response RunResponse(Type pageType, Request request, params string[] arguments)
        {
            var routingInfo = new RoutingInfo { Request = request, SelectedPageType = pageType, Arguments = arguments };
            return RunWithMiddleware(
                routingInfo,
                _middleware.Concat(new[] { new TerminalMiddleware(() => _pageCreator.Create(routingInfo)) }));
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