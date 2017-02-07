using System;
using System.Linq;

namespace Starcounter.Authorization.Routing
{
    /// <summary>
    /// Contains helper methods for <see cref="IPageContext{T}"/> handling
    /// </summary>
    public class PageContextSupport
    {
        /// <summary>
        /// Returns the appropriate context type for given page type. If the page implements <see cref="IPageContext{T}"/>
        /// or <see cref="IBound{T}"/> its type parameter is used to determine the context type.
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns>Context type if it can be determined, null otherwise</returns>
        public Type GetContextType(Type pageType)
        {
            return pageType.GetInterface($"{nameof(IPageContext<int>)}`1")?.GetGenericArguments().First()
                   ?? pageType.GetInterface($"{nameof(IBound<int>)}`1")?.GetGenericArguments().First();
        }

        /// <summary>
        /// Take appropriate action to bind the context to the page. This usually involves setting <see cref="Json.Data"/> 
        /// property if the page is <see cref="IBound{T}"/> or calling <see cref="IPageContext{T}.HandleContext"/>
        /// </summary>
        /// <param name="page">The page that should handle the context</param>
        /// <param name="context">The context object. Its type should match the context type of the page</param>
        public static void HandleContext(object page, object context)
        {
            // todo cache the reflection
            // `int` parameter is ignored by nameof operator
            if (page.GetType().GetInterface($"{nameof(IPageContext<int>)}`1") != null)
            {
                page.GetType().GetMethod(nameof(IPageContext<int>.HandleContext)).Invoke(page, new[] {context});
                return;
            }
            if (page.GetType().GetInterface($"{nameof(IBound<int>)}`1") != null)
            {
                page.GetType().GetProperty(nameof(Json.Data)).SetValue(page, context);
                return;
            }
            
            throw new Exception($"Could not handle context '{context}' for page '{page}': " +
                                $"Please mark {page.GetType()} as {typeof(IPageContext<>)} or IBound to a database type");
        }
    }
}