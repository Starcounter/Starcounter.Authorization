using System;
using System.Linq.Expressions;
using Starcounter.Authorization.Middleware;

namespace Starcounter.Authorization
{
    public class SecurityMiddlewareOptions
    {
        public Func<Type, Expression, Expression, Expression> CheckDeniedHandler { get; set; } =
            PageSecurity.PageSecurity.CreateThrowingDeniedHandler<Exception>();

        /// <summary>
        /// Used to return a response when current user is denied access. String argument is the relative URI of the original request.
        /// Default value is provided by <see cref="DefaultSecurityMiddlewareOptions"/>
        /// </summary>
        public Func<string, Json> UnauthenticatedResponseCreator { get; set; }

        /// <summary>
        /// Used to return a response when current user isn't authorized to the requested resource. String argument is the relative URI of the original request.
        /// Default value is provided by <see cref="DefaultSecurityMiddlewareOptions"/>
        /// </summary>
        public Func<string, Response> UnauthorizedResponseCreator { get; set; }

        public SecurityMiddlewareOptions WithCheckDeniedHandler(
            Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            CheckDeniedHandler = checkDeniedHandler;
            return this;
        }

        public SecurityMiddlewareOptions WithUnauthenticatedResponseCreator(Func<string, Json> unauthenticatedResponseCreator)
        {
            UnauthenticatedResponseCreator = unauthenticatedResponseCreator;
            return this;
        }
    }
}