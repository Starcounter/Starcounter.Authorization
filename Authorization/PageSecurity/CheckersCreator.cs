using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.PageSecurity
{
    /// <summary>
    /// Governs creation of Action&lt;&gt; objects that perform the actual checks
    /// </summary>
    public class CheckersCreator
    {
        private readonly IAuthorizationEnforcement _authorizationEnforcement;
        private readonly IAttributeRequirementsResolver _attributeRequirementsResolver;

        public CheckersCreator(IAuthorizationEnforcement authorizationEnforcement,
            IAttributeRequirementsResolver attributeRequirementsResolver)
        {
            _authorizationEnforcement = authorizationEnforcement;
            _attributeRequirementsResolver = attributeRequirementsResolver;
        }

        public Check WrapCheck(Check existingCheck, Type pageType, int numberOfLayers)
        {
            if (existingCheck == null || existingCheck.AllowAnonymous)
            {
                return existingCheck;
            }
            var pageParameterExpression = Expression.Parameter(pageType, "page");
            Expression pageExpression = pageParameterExpression;
            var parentMemberInfo = typeof(Json).GetProperty(nameof(Json.Parent));
            foreach (var i in Enumerable.Range(0, numberOfLayers))
            {
                pageExpression = Expression.MakeMemberAccess(pageExpression, parentMemberInfo);
            }
            var existingPageType = existingCheck.PageType;
            var checkLambda = Expression.Lambda(
                Expression.Invoke(
                    Expression.Constant(existingCheck.CheckAction),
                    Expression.Convert(pageExpression, existingPageType)),
                pageParameterExpression).Compile();
            return new Check(pageType, checkLambda);
        }

        /// <summary>
        /// Creates an Func&lt;pageType, bool&gt; that checks permissions and uses <see cref="checkDeniedHandler"/> and returns false in case it's denied. Returns true if granted
        /// </summary>
        /// <param name="pageType">Type of page which determines the context of check</param>
        /// <param name="attributeProvider">A page class or its method (a input handler) that could have authorization attributes on it</param>
        /// <param name="checkDeniedHandler"></param>
        /// <returns>an object of type Func&lt;pageType, bool&gt; that either returns true if access is granted or performs <see cref="checkDeniedHandler"/> and returns false. Null if no check needs to be performed</returns>
        public Check CreateThrowingCheckFromExistingPage(Type pageType,
            ICustomAttributeProvider attributeProvider,
            Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            if (attributeProvider
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any())
            {
                return Check.CreateAllowAnonymous();
            }

            var authorizeAttributes = attributeProvider.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();
            if (!authorizeAttributes.Any())
            {
                return null;
            }

            var requirements = _attributeRequirementsResolver.ResolveAsync(authorizeAttributes).Result.ToList();

            Expression resourceExpression;
            var appParameterExpression = Expression.Parameter(pageType, "page");
            {
                Func<Json, object> getData = json =>
                {
                    while (json.Data == null && json.Parent != null)
                    {
                        json = json.Parent;
                    }

                    return json.Data;
                };
                resourceExpression = Expression.Invoke(Expression.Constant(getData), appParameterExpression);
            }

            try
            {
                var checkResult = Expression.Parameter(typeof(bool));
                var checkLambda = Expression.Lambda(Expression.Block(
                        new[] { checkResult },
                        Expression.Assign(checkResult, Expression.MakeMemberAccess(
                            CreateCheckPermissionCall(requirements, resourceExpression),
                            // ReSharper disable once AssignNullToNotNullAttribute
                            typeof(Task<bool>).GetProperty(nameof(Task<bool>.Result)))),
                        Expression.IfThen(
                            Expression.IsFalse(checkResult),
                            checkDeniedHandler(pageType, resourceExpression, appParameterExpression)),
                        checkResult),
                    appParameterExpression).Compile();
                return new Check(pageType, checkLambda);
                // reflection based body, for reference:
                //            return (PageType page) => {
                //                bool checkResult = [CreateCheckPermissionCall(requirements, getData(page)];
                //                if(!checkResult)
                //                {
                //                    [_checkDeniedHandler(PageType, getData(page), page)]
                //                }
                //                return checkResult;
                //            };
            }
            catch (Exception ex)
            {
                // todo test
                throw new Exception(
                    $"Could not create check for page {pageType} and permission {resourceExpression.Type}. Make sure your provided checkDeniedHandler returns correct Expression",
                    ex);
            }
        }

        /// <summary>
        /// Creates a Func that accepts a resource (AKA context) and returns true if permission is granted. Returns null if no check is necessary
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns></returns>
        public Func<object, Task<bool>> CreateBoolCheck(Type pageType)
        {
            var authorizeAttributes = pageType.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();
            if (!authorizeAttributes.Any())
            {
                return null;
            }
            ParameterExpression contextParameterExpression = Expression.Parameter(typeof(object), "context");

            return Expression.Lambda<Func<object, Task<bool>>>(
                    CreateCheckPermissionCall(_attributeRequirementsResolver.ResolveAsync(authorizeAttributes).Result, contextParameterExpression),
                    contextParameterExpression)
                .Compile();
        }

        private MethodCallExpression CreateCheckPermissionCall(IEnumerable<IAuthorizationRequirement> requirements, Expression resourceExpression)
        {
            var checkPermissionMethod = typeof(IAuthorizationEnforcement)
                .GetMethod(nameof(IAuthorizationEnforcement.CheckRequirementsAsync));
            var checkPermissionCall = Expression.Call(
                Expression.Constant(_authorizationEnforcement),
                checkPermissionMethod,
                Expression.Constant(requirements),
                resourceExpression);
            return checkPermissionCall;
        }
    }
}