using System;
using System.Reflection;
using Starcounter.Advanced;

namespace Starcounter.Authorization.Routing.Middleware
{
    /// <summary>
    /// Responsible for obtaining the page context using information from URI.
    /// </summary>
    /// This middleware will set the <see cref="RoutingInfo.Context"/> property in current <see cref="RoutingInfo"/>
    /// for a page that implements either <see cref="IPageContext{T}"/> or <see cref="IBound{T}"/>. If the selected page type
    /// has a method marked with <see cref="UriToContextAttribute"/>, then it will be used. Otherwise, this middleware will
    /// attempt to retrieve the context from database.
    public class ContextMiddleware: IPageMiddleware
    {
        private readonly PageContextSupport _pageContextSupport = new PageContextSupport();
        private readonly IObjectRetriever _objectRetriever;

        /// <summary>
        /// </summary>
        /// <param name="objectRetriever">This is used primarily by tests to "mock" the db access. Leave null to use DbHelper.FromID</param>
        public ContextMiddleware(IObjectRetriever objectRetriever = null)
        {
            _objectRetriever = objectRetriever ?? new DatabaseObjectRetriever();
        }

        public Response Run(RoutingInfo routingInfo, Func<Response> next)
        {
            var contextType = _pageContextSupport.GetContextType(routingInfo.SelectedPageType);
            if (contextType == null)
            {
                return next();
            }

            var context = CreateContextFromUri(routingInfo.SelectedPageType, contextType, routingInfo.Arguments);
            if (context == null)
            {
                return Response.FromStatusCode(404);
            }
            routingInfo.Context = context;

            return next();
        }

        private object CreateContextFromUri(Type pageType, Type contextType, string[] arguments)
        {
            MethodInfo explicitUriToContext = GetExplicitUriToContext(pageType, contextType);
            if (explicitUriToContext != null)
            {
                return explicitUriToContext.Invoke(null, new object[] { arguments });
            }
            // i.e. is database object
            if (typeof(IBindable).IsAssignableFrom(contextType))
            {
                if (arguments.Length != 1)
                {
                    throw new Exception($"Could not create context of type {contextType} for page of type {pageType}: " +
                                        $"There should be exactly one URI argument that is ID of {contextType} object in DB. " +
                                        $"If you want to create context manually use {nameof(UriToContextAttribute)}");
                }
                return _objectRetriever.GetById(contextType, arguments[0]);
            }
            throw new Exception($"Could not create context of type {contextType} for page {pageType}: " +
                                $"Please mark {pageType} as IBound to a database type or use {nameof(UriToContextAttribute)}");
        }

        private MethodInfo GetExplicitUriToContext(Type pageType, Type contextType)
        {
            return ReflectionHelper.GetStaticMethodWithAttribute(pageType, typeof(UriToContextAttribute), typeof(string[]), contextType);
        }

        public interface IObjectRetriever
        {
            object GetById(Type desiredType, string id);
        }

        private class DatabaseObjectRetriever : IObjectRetriever
        {
            public object GetById(Type desiredType, string id)
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
                var obj = DbHelper.FromID(objectId);
                return desiredType.IsInstanceOfType(obj) ? obj : null;
            }
        }
    }
}