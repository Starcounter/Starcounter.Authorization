using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;
using Starcounter.Templates;

namespace Starcounter.Authorization.PageSecurity
{
    public class PageSecurity
    {
        private readonly List<Type> _enhancedTypes = new List<Type>();
        private readonly CheckersCreator _checkersCreator;
        private readonly HandlersCreator _handlersCreator = new HandlersCreator();

        public PageSecurity(IAuthorizationEnforcement authorizationEnforcement)
        {
            _checkersCreator = new CheckersCreator(authorizationEnforcement, (type, type1, arg3) => Expression.Throw(Expression.New(typeof(UnauthorizedException))));
        }

        public void EnhanceClass(Type pageType)
        {
            if (_enhancedTypes.Contains(pageType))
            {
                return;
            }
            _enhancedTypes.Add(pageType);

            var allHandlersTasks = CreateTaskList(pageType);
            AddHandlers(pageType, allHandlersTasks);
        }

        public bool CheckClass(Type pageType, object data)
        {
            // todo cache
            return _checkersCreator.CreateBoolCheck(pageType, pageType, () => data)();
        }

        private void AddHandlers(Type pageType, IEnumerable<Tuple<MethodInfo, Template, object>> allHandlersTasks)
        {
            foreach (var tuple in allHandlersTasks)
            {
                try
                {
                    // tuple.Item1 is the existing Handle() method (optional)
                    var originalHandlerMethod = tuple.Item1;
                    // tuple.Item2 is the Property that is being handled
                    var property = tuple.Item2;
                    // tuple.Item3 is the Action<pageType> that performs the actual permission check
                    var checkAction = tuple.Item3;

                    // "T" in Property<T>
                    var propertyType = tuple.Item2.GetType().GetProperty(nameof(Property<int>.DefaultValue)).PropertyType;

                    var createInputEvent = originalHandlerMethod != null
                        ? _handlersCreator.RecreateCreateInputEvent(originalHandlerMethod, propertyType)
                        : _handlersCreator.CreateEmptyInputEvent(propertyType);
                    property.GetType().GetMethod(nameof(Property<int>.AddHandler)).Invoke(
                        property,
                        new[]
                        {
                            createInputEvent,
                            _handlersCreator.CreateHandler(originalHandlerMethod, propertyType, pageType, checkAction)
                        });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not generate handler for property {tuple.Item2}", ex);
                }
            }
        }

        /// <summary>
        /// Creates an IEnumerable of tasks to perform. Each entry represents a property (Template) that has
        /// to have a handler added, with permission check (object, underlying Action&lt;pageType&gt;)
        /// and an optional originalHandler method (MethodInfo) to invoke after the check
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns></returns>
        private IEnumerable<Tuple<MethodInfo, Template, object>> CreateTaskList(Type pageType)
        {
            // the name "DefaultTemplate" is defined inside each Page class, so couldn't be obtained with nameof
            TObject pageDefaultTemplate;
            try
            {
                pageDefaultTemplate = (TObject)pageType.GetField("DefaultTemplate").GetValue(null);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not access DefaultTemplate of page class {pageType}. Make sure you passed correct type to {nameof(EnhanceClass)}",
                    ex);
            }

            // underlying type is Action<pageType>
            object pageCheck = _checkersCreator.CreateThrowingCheck(pageType, pageType);

            var pageProperties = pageDefaultTemplate.Properties;
            var existingHandlers = pageType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "Handle")
                .Select(method => Tuple.Create(method, FindPropertyByHandlerMethod(method, pageProperties)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();

            var existingHandlersList = existingHandlers.ToList();
            var handlersWithChecks = existingHandlersList
                .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, _checkersCreator.CreateThrowingCheck(pageType, tpl.Item1)))
                .Where(tuple => tuple.Item3 != null)
                .ToList();

            IEnumerable<Tuple<MethodInfo, Template, object>> allHandlersTasks = handlersWithChecks;
            if (pageCheck != null)
            {
                var handlersWithoutOwnChecks = existingHandlersList
                    .Except(handlersWithChecks.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2)))
                    .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, pageCheck))
                    .ToList();

                var propertiesWithoutHandlers = pageProperties
                    .Except(existingHandlers.Select(tuple => tuple.Item2))
                    .Select(template => Tuple.Create<MethodInfo, Template, object>(null, template, pageCheck));

                allHandlersTasks = allHandlersTasks
                    .Concat(handlersWithoutOwnChecks)
/*                    .Concat(propertiesWithoutHandlers)*/;
            }
            return allHandlersTasks;
        }

        private Template FindPropertyByHandlerMethod(MethodInfo handler, PropertyList candidates)
        {
            try
            {
                var propertyName = handler.GetParameters().FirstOrDefault()?.ParameterType.Name;
                return propertyName == null ? null : candidates[propertyName];
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not determine property for handler: {handler}", ex);
            }
        }

        private static object InvokePrivateGenericMethod(object @this, string name, Type[] typeParameter, params object[] arguments)
        {
            return @this.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeParameter)
                .Invoke(@this, arguments);
        }

        /// <summary>
        /// Governs creation of Action&lt;&gt; objects that perform the actual checks
        /// </summary>
        private class CheckersCreator
        {
            private readonly IAuthorizationEnforcement _authorizationEnforcement;
            private readonly Func<Type, Expression, Expression, Expression> _checkDeniedHandler;

            public CheckersCreator(IAuthorizationEnforcement authorizationEnforcement, Func<Type, Expression, Expression, Expression> checkDeniedHandler)
            {
                _authorizationEnforcement = authorizationEnforcement;
                _checkDeniedHandler = checkDeniedHandler;
            }

            /// <summary>
            /// Creates an Action&lt;pageType&gt; that checks permissions and throws <see cref="UnauthorizedException"/> in case it's denied
            /// as well as loading the page
            /// </summary>
            /// <param name="pageType">Type of page which determines the context of check</param>
            /// <param name="attributeProvider">A page class or its method (a input handler) that could have authorization attributes on it</param>
            /// <returns>an object of type Action&lt;pageType&gt; that throws <see cref="UnauthorizedException"/> if access is denied. Null if no check needs to be performed</returns>
            public object CreateThrowingCheck(Type pageType, ICustomAttributeProvider attributeProvider)
            {
                var appParameterExpression = Expression.Parameter(pageType, "app");

                var permissionExpression = CreatePermissionExpression(pageType, attributeProvider, Expression.MakeMemberAccess(
                    appParameterExpression,
                    pageType.GetProperty("Data", GetDataTypeForPage(pageType))));

                if (permissionExpression == null)
                {
                    return null;
                }

                return CreateCheckLambdaFromExistingPage(
                    pageType,
                    permissionExpression,
                    appParameterExpression);
            }

            public Func<bool> CreateBoolCheck(Type pageType, ICustomAttributeProvider attributeProvider, Func<object> argumentGetter)
            {
                var permissionExpression = CreatePermissionExpression(pageType, attributeProvider, Expression.Invoke(Expression.Constant(argumentGetter)));

                if (permissionExpression == null)
                {
                    return null;
                }

                return Expression.Lambda<Func<bool>>(CreateCheckPermissionCall(permissionExpression)).Compile();
            }

            /// <summary>
            /// Creates an Expression that represents permission required by attributes in <see cref="attributesSource"/> that are defined either
            /// on page of type <see cref="pageType"/> or its handlers
            /// </summary>
            /// <param name="pageType"></param>
            /// <param name="attributesSource"></param>
            /// <param name="permissionArgumentExpression"></param>
            /// <returns></returns>
            private Expression CreatePermissionExpression(Type pageType, ICustomAttributeProvider attributesSource, Expression permissionArgumentExpression)
            {
                var requirePermissionDataAttribute =
                    attributesSource.GetCustomAttributes(typeof(RequirePermissionDataAttribute), true)
                    .Cast<RequirePermissionDataAttribute>()
                    .FirstOrDefault();
                var requirePermissionAttribute = attributesSource.GetCustomAttributes(typeof(RequirePermissionAttribute), true)
                    .Cast<RequirePermissionAttribute>()
                    .FirstOrDefault();

                if (requirePermissionAttribute != null && requirePermissionDataAttribute != null)
                {
                    // todo test
                    throw new Exception(
                        $"There should be only one of attributes: {nameof(RequirePermissionDataAttribute)} or {nameof(RequirePermissionAttribute)} in {attributesSource}");
                }
                if (requirePermissionDataAttribute != null)
                {
                    var requiredPermission = requirePermissionDataAttribute.RequiredPermission;
                    var dataTypeOfPage = GetDataTypeForPage(pageType);
                    if (dataTypeOfPage == null)
                    {
                        // todo test
                        throw new ArgumentException($"Invalid usage of {nameof(RequirePermissionDataAttribute)} on {attributesSource}: Page {pageType} is not IBound");
                    }
                    if (requiredPermission.GetConstructor(new[] {dataTypeOfPage}) == null)
                    {
                        // todo test (2)
                        throw new ArgumentException($"Invalid usage of {nameof(RequirePermissionDataAttribute)} on {attributesSource}: Required permission {requiredPermission} has no ctor that accepts {dataTypeOfPage} as argument");
                    }
                    return CreatePermissionWithParameter(pageType, requiredPermission, permissionArgumentExpression);
                }
                if (requirePermissionAttribute != null)
                {
                    var requiredPermission = requirePermissionAttribute.RequiredPermission;
                    if (requiredPermission.GetConstructor(new Type[0]) == null)
                    {
                        // todo test
                        throw new ArgumentException($"Invalid usage of {nameof(RequirePermissionAttribute)} on {attributesSource}: Required permission {requiredPermission} has no default ctor");
                    }
                    return CreateParameterlessPermission(requiredPermission);
                }

                return null;
            }

            /// <summary>
            /// Creates an Action&lt;pageType&gt; that would accept an existing page and act accordingly to <see cref="_checkDeniedHandler"/>
            /// </summary>
            /// <param name="pageType"></param>
            /// <param name="permissionExpression"></param>
            /// <param name="appParameterExpression"></param>
            /// <returns></returns>
            private object CreateCheckLambdaFromExistingPage(Type pageType, Expression permissionExpression, ParameterExpression appParameterExpression)
            {
                var checkPermissionCall = CreateCheckPermissionCall(permissionExpression);

                var body = Expression.IfThen(
                    Expression.IsFalse(checkPermissionCall),
                    _checkDeniedHandler(pageType, permissionExpression, appParameterExpression));
                try
                {
                    return Expression.Lambda(body, appParameterExpression).Compile();
                }
                catch (Exception ex)
                {
                    // todo test
                    throw new Exception($"Could not create check for page {pageType} and permission {permissionExpression.Type}. Make sure your provided checkDeniedHandler returns Expression<Action<TApp>>", ex);
                }
            }

            private MethodCallExpression CreateCheckPermissionCall(Expression permissionExpression)
            {
                var checkPermissionMethod = typeof(IAuthorizationEnforcement)
                    .GetMethod(nameof(IAuthorizationEnforcement.CheckPermission))
                    .MakeGenericMethod(permissionExpression.Type);
                var checkPermissionCall = Expression.Call(
                    Expression.Constant(_authorizationEnforcement),
                    checkPermissionMethod,
                    permissionExpression);
                return checkPermissionCall;
            }

            private Expression CreatePermissionWithParameter(Type pageType, Type permissionType, Expression permissionArgumentExpression)
            {
                var dataType = GetDataTypeForPage(pageType);
                try
                {
                    var permissionCtor = permissionType.GetConstructor(new[] { dataType });
                    if (permissionCtor == null)
                    {
                        // CreateThrowingCheck already checks if suitable ctor exists, but double-check shouldn't hurt
                        throw new Exception($"Could not find suitable ctor for type {permissionType}. Make sure it has ctor that accepts {dataType} as argument");
                    }

                    return Expression.New(
                        permissionCtor, 
                        permissionArgumentExpression);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not generate Data-based authorization check for page class {pageType}", ex);
                }
            }

            private Expression CreateParameterlessPermission(Type permissionType)
            {
                // CreateThrowingCheck already checks if suitable ctor exists
                return Expression.New(permissionType);
            }

            private static Type GetDataTypeForPage(Type pageType)
            {
                try
                {
                    return pageType.GetInterface($"{nameof(IBound<int>)}`1")?.GetGenericArguments().First();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Could not determine Data type for page class {pageType}. Please make sure you marked it with IBound interface",
                        ex);
                }
            }
        }

        /// <summary>
        /// Governs creation of arguments to <see cref="Property{T}.AddHandler"/> method, namely 'createInputEvent' and 'handler'
        /// </summary>
        private class HandlersCreator
        {
            /// <summary>
            /// Generate empty createInputEvent
            /// </summary>
            /// <param name="propertyType"></param>
            /// <returns></returns>
            public object CreateEmptyInputEvent(Type propertyType)
            {
                return InvokePrivateGenericMethod(this, nameof(CreateEmptyInputEventTemplate), new[] { propertyType });
            }

            private Func<Json, Property<T>, T, Input<T>> CreateEmptyInputEventTemplate<T>()
            {
                return (Json pup, Property<T> prop, T value) => new Input<T>()
                {
                    Value = value
                };
            }

            /// <summary>
            /// Creates handler that performs checkAction and then delegates to originalHandler
            /// </summary>
            /// <param name="originalHandler"></param>
            /// <param name="propertyType"></param>
            /// <param name="pageType"></param>
            /// <param name="checkAction">This should be of type Action&lt;pageType&gt;</param>
            /// <returns></returns>
            public object CreateHandler(MethodInfo originalHandler, Type propertyType, Type pageType, object checkAction)
            {
                return InvokePrivateGenericMethod(this, nameof(HandlerTemplate), new[] { propertyType, pageType }, originalHandler,
                    checkAction);
            }

            /// <summary>
            /// Template for 'handler' function. Performs checkAction and then calls originalHandler (if not null)
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TApp"></typeparam>
            /// <param name="originalHandler">original Handler(Input.) method. Ignored if null</param>
            /// <param name="checkAction"></param>
            /// <returns></returns>
            private Action<Json, Input<T>> HandlerTemplate<T, TApp>(MethodInfo originalHandler, Action<TApp> checkAction) where TApp : Json
            {
                var jsonParameter = Expression.Parameter(typeof(Json), "json");
                var inputParameter = Expression.Parameter(typeof(Input<T>), "input");
                var jsonAsTApp = Expression.Convert(jsonParameter, typeof(TApp));
                var checkInvocation = Expression.Invoke(Expression.Constant(checkAction), jsonAsTApp);
                var inputType = originalHandler.GetParameters().First().ParameterType; // Input.<propertyName>
                var originalHandlerCall = Expression.Call(jsonAsTApp, originalHandler, Expression.Convert(inputParameter, inputType));

                Expression body = originalHandler == null ? (Expression)checkInvocation : Expression.Block(checkInvocation, originalHandlerCall);
                return Expression.Lambda<Action<Json, Input<T>>>(body, jsonParameter, inputParameter).Compile();
                // reflection based body, for reference:
                //            return (Json pup, Input<T> input) => {
                //                var page = (TApp)pup;
                //                checkAction(page);
                //
                //                if (originalHandler != null)
                //                {
                //                    originalHandler.Invoke(page, new object[] { input });
                //                }
                //            };
            }

            /// <summary>
            /// Creates createInputEvent that is equivalent to one generated originally by Starcounter VS plugin.
            /// This method exists, because it's easier to recreate it then access the one already created.
            /// </summary>
            /// <param name="originalHandler"></param>
            /// <param name="propertyType"></param>
            /// <returns></returns>
            public object RecreateCreateInputEvent(MethodInfo originalHandler, Type propertyType)
            {
                // Input.<PropertyName>
                var tInput = originalHandler.GetParameters().First().ParameterType;
                // TString / TBool / etc.
                var tTemplate = tInput.GetProperty(nameof(Input<Json, TString, string>.Template))?.PropertyType;
                if (tTemplate == null)
                {
                    throw new Exception("tTemplate is null");
                }
                // page class
                var tApp = tInput.GetProperty(nameof(Input<Json, TString, string>.App))?.PropertyType;
                if (tApp == null)
                {
                    throw new Exception("tApp is null");
                }
                return InvokePrivateGenericMethod(this, nameof(CreateInputEventTemplate),
                    new[] { propertyType, tInput, tApp, tTemplate });
            }

            /// <summary>
            /// This is a template used to generate CreateInputEvent method to pass to AddHandler
            /// </summary>
            private Func<Json, Property<T>, T, Input<T>> CreateInputEventTemplate<T, TInput, TApp, TTemplate>() where TInput : Input<TApp, TTemplate, T>, new() where TApp : Json where TTemplate : Property<T>
            {
                return (json, property, value) => new TInput
                {
                    Value = value,
                    App = (TApp)json,
                    Template = (TTemplate)property
                };
            }
        }
    }
}