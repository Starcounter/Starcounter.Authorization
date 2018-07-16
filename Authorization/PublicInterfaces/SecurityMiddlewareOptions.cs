using System;
using System.Linq.Expressions;

namespace Starcounter.Authorization
{
    public class SecurityMiddlewareOptions
    {

        public Func<Type, Expression, Expression, Expression> CheckDeniedHandler { get; set; } =
            PageSecurity.PageSecurity.CreateThrowingDeniedHandler<Exception>();

        public SecurityMiddlewareOptions WithCheckDeniedHandler(
            Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            CheckDeniedHandler = checkDeniedHandler;
            return this;
        }
    }
}