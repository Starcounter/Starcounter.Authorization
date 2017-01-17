using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;
using Starcounter.Templates;
using Check = System.Tuple<System.Type, object>;

namespace Starcounter.Authorization.PageSecurity
{
    public class PageSecurity
    {
        private readonly List<Type> _enhancedTypes = new List<Type>();
        private readonly CheckersCreator _checkersCreator;
        private readonly HandlersCreator _handlersCreator = new HandlersCreator();

        /// <summary>
        /// </summary>
        /// <param name="authorizationEnforcement"></param>
        /// <param name="checkDeniedHandler">This determines what happens when access is denied inside input handler.
        /// It accepts type of page in which the handler is defined,
        /// Expression representing the permission that has been denied,
        /// Expression representing the page itself
        /// and should return an Expression representing action to be undertaken
        /// <see cref="CreateThrowingDeniedHandler{T}"/></param>
        public PageSecurity(IAuthorizationEnforcement authorizationEnforcement, Func<Type, Expression, Expression, Expression> checkDeniedHandler)
        {
            _checkersCreator = new CheckersCreator(authorizationEnforcement, checkDeniedHandler);
        }

        /// <summary>
        /// Create a checkDeniedHandler (to be used in ctor) that will throw a new Exception of type T
        /// whenever access denied inside input handler.
        /// </summary>
        /// <typeparam name="T">The type of the exception to be thrown</typeparam>
        /// <returns>A ready-to-use handler to be supplied to the <see cref="PageSecurity"/> constructor</returns>
        public static Func<Type, Expression, Expression, Expression> CreateThrowingDeniedHandler<T>()
            where T : Exception, new()
        {
            return (pageType, permissionExpression, pageExpression) => Expression.Throw(Expression.New(typeof(T)));
        }

        /// <summary>
        /// Enhances the specified page class, adding security checks to all of its input handlers.
        /// Will do nothing if this class has already been enhanced by this instance of PageSecurity
        /// </summary>
        /// <param name="pageType">The page class to be enhanced, must derive from <see cref="Json"/></param>
        public void EnhanceClass(Type pageType)
        {
            if (_enhancedTypes.Contains(pageType))
            {
                return;
            }
            _enhancedTypes.Add(pageType);

            var allHandlersTasks = CreateTaskList(pageType);
            AddHandlers(allHandlersTasks);
        }

        /// <summary>
        /// Checks if the current user has access to a page of specified type.
        /// </summary>
        /// <param name="pageType">The type of page that will be inspected</param>
        /// <param name="objects">Database objects that would be required to construct the page object. Usually will be either empty or contain one object of the type that this page is IBound to</param>
        /// <returns>true if the current user has access to this page or no check was necessary, false otherwise</returns>
        public bool CheckClass(Type pageType, object[] objects)
        {
            // todo cache
            var check = _checkersCreator.CreateBoolCheck(pageType, pageType);
            return check == null || check(objects);
        }

        private void AddHandlers(IEnumerable<Tuple<MethodInfo, Template, Check, Type>> allHandlersTasks)
        {
            foreach (var tuple in allHandlersTasks)
            {
                try
                {
                    // tuple.Item1 is the existing Handle() method (optional)
                    var originalHandlerMethod = tuple.Item1;
                    // tuple.Item2 is the Property that is being handled
                    var property = tuple.Item2;
                    // tuple.Item3 is the factory for expression that will do the check. It accepts a LabelTarget pointing at return label and Expression representing app (aka page) param
                    var checkAction = tuple.Item3;
                    // tuple.Item4 is the pageType
                    var pageType = tuple.Item4;

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
        /// <param name="parentPageType"></param>
        /// <returns></returns>
        private IEnumerable<Tuple<MethodInfo, Template, Check, Type>> CreateTaskList(Type pageType, Type parentPageType = null)
        {
            TObject pageDefaultTemplate;
            try
            {
                // the name "DefaultTemplate" is defined inside each Page class, so couldn't be obtained with nameof
                pageDefaultTemplate = (TObject)pageType.GetField("DefaultTemplate").GetValue(null);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not access DefaultTemplate of page class {pageType}. Make sure you passed correct type to {nameof(EnhanceClass)}",
                    ex);
            }

            return AllHandlersTasks(pageType, pageDefaultTemplate.Children);
        }

        /// <summary>
        /// Generates tasks for a given page
        /// </summary>
        /// <param name="pageType">Class of the page that is inspected (the class that is written by the developer)</param>
        /// <param name="properties">Properties that belong to this page</param>
        /// <param name="parentCheck">If present, the Check that was generated for the parent page</param>
        /// <returns>Each tuple contains: 
        /// original handler (or null), 
        /// a property to attach to, 
        /// a check to perform (see Check definition on top of file),
        /// the class of the page that this property belongs to</returns>
        private IEnumerable<Tuple<MethodInfo, Template, Check, Type>> AllHandlersTasks(Type pageType, IEnumerable<Template> properties, Check parentCheck = null)
        {
            var pageProperties = properties.ToList();
            Check pageCheck = _checkersCreator.CreateThrowingCheckFromExistingPage(pageType, pageType);
            if (pageCheck == null && parentCheck != null)
            {
                pageCheck = parentCheck;
            }

            var existingHandlers = pageType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "Handle")
                .Select(method => Tuple.Create(method, FindPropertyByHandlerMethod(method, pageProperties)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();

            var existingHandlersList = existingHandlers.ToList();
            var handlersWithChecks = existingHandlersList
                .Select(
                    tpl =>
                        Tuple.Create(tpl.Item1, tpl.Item2,
                            _checkersCreator.CreateThrowingCheckFromExistingPage(pageType, tpl.Item1), pageType))
                .Where(tuple => tuple.Item3 != null)
                .ToList();

            // todo rewrite to use .ElementType.InstanceType and get info from Schema, not GetGenericTypeParameter
            var arrayProperties = pageProperties
                .Select(template => Tuple.Create(template, GetGenericTypeParameter(template, typeof(TArray<>))))
                .Where(tuple => tuple.Item2 != null)
                .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item2.GetField("DefaultTemplate").GetValue(null) as TObject))
                .Where(tuple => tuple.Item3 != null)
                .SelectMany(tuple => AllHandlersTasks(tuple.Item2, tuple.Item3.Children, _checkersCreator.WrapCheck(pageCheck, tuple.Item2, 2)));

            var subPages = pageProperties
                .OfType<TObject>()
                .SelectMany(o => AllHandlersTasks(o.InstanceType, o.Children, _checkersCreator.WrapCheck(pageCheck, o.InstanceType, 1)));

            IEnumerable<Tuple<MethodInfo, Template, Check, Type>> allHandlersTasks = handlersWithChecks
                .Concat(subPages)
                .Concat(arrayProperties);

            if (pageCheck != null)
            {
                var handlersWithoutOwnChecks = existingHandlersList
                    .Except(handlersWithChecks.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2)))
                    .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, pageCheck, pageType))
                    .ToList();

                var propertiesWithoutHandlers = pageProperties
                    .Except(existingHandlers.Select(tuple => tuple.Item2))
                    .Where(IsProperty)
                    .Select(template => Tuple.Create<MethodInfo, Template, Check, Type>(null, template, pageCheck, pageType));

                allHandlersTasks = allHandlersTasks
                    .Concat(handlersWithoutOwnChecks)
                    .Concat(propertiesWithoutHandlers);
            }
            return allHandlersTasks;
        }

        private static bool IsProperty(object instance)
        {
            return GetGenericTypeParameter(instance, typeof(Property<>)) != null;
        }

        /// <summary>
        /// If the <see cref="instance"/> is of type <see cref="genericType"/> returns its first type parameter. Otherwise null
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        private static Type GetGenericTypeParameter(object instance, Type genericType)
        {
            var type = instance.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                {
                    return type.GetGenericArguments()[0];
                }
                type = type.BaseType;
            }
            return null;
        }

        private static Template FindPropertyByHandlerMethod(MethodInfo handler, IEnumerable<Template> candidates)
        {
            try
            {
                var propertyName = handler.GetParameters().FirstOrDefault()?.ParameterType.Name;
                return propertyName == null ? null : candidates.First(template => template.PropertyName == propertyName);
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

            public Check WrapCheck(Check existingCheck, Type pageType, int numberOfLayers)
            {
                var pageParameterExpression = Expression.Parameter(pageType, "page");
                Expression pageExpression = pageParameterExpression;
                var parentMemberInfo = typeof(Json).GetProperty(nameof(Json.Parent));
                foreach (var i in Enumerable.Range(0, numberOfLayers))
                {
                    pageExpression = Expression.MakeMemberAccess(pageExpression, parentMemberInfo);
                }
                var existingPageType = existingCheck.Item1;
                var checkLambda = Expression.Lambda(
                    Expression.Invoke(
                        Expression.Constant(existingCheck.Item2),
                        Expression.Convert(pageExpression, existingPageType)),
                    pageParameterExpression).Compile();
                return Tuple.Create<Type, object>(pageType, checkLambda);
            }

            /// <summary>
            /// Creates an Func&lt;pageType, bool&gt; that checks permissions and uses <see cref="_checkDeniedHandler"/> and returns false in case it's denied. Returns true if granted
            /// </summary>
            /// <param name="pageType">Type of page which determines the context of check</param>
            /// <param name="attributeProvider">A page class or its method (a input handler) that could have authorization attributes on it</param>
            /// <returns>an object of type Func&lt;pageType, bool&gt; that either returns true if access is granted or performs <see cref="_checkDeniedHandler"/> and returns false. Null if no check needs to be performed</returns>
            public Check CreateThrowingCheckFromExistingPage(Type pageType, ICustomAttributeProvider attributeProvider)
            {
                var permissionsConstructor = GetRequiredPermissionsConstructor(pageType, attributeProvider);
                if (permissionsConstructor == null)
                {
                    return null;
                }

                Expression permissionExpression;
                var appParameterExpression = Expression.Parameter(pageType, "page");
                var parameterTypes =
                    permissionsConstructor.GetParameters().Select(param => param.ParameterType).ToList();
                switch (parameterTypes.Count)
                {
                    case 0:
                        permissionExpression = Expression.New(permissionsConstructor);
                        break;
                    case 1:
                        var paramType = parameterTypes.First();
                        Func<Json, object> getData = json =>
                        {
                            while (!paramType.IsInstanceOfType(json.Data) && json.Parent != null)
                            {
                                json = json.Parent;
                            }

                            return json.Data;
                        };
                        permissionExpression = Expression.New(permissionsConstructor,
                            Expression.Convert(Expression.Invoke(Expression.Constant(getData), appParameterExpression), paramType));
                        break;
                    default:
                        throw new Exception(
                            $"Could not create check for page {pageType} and permission {permissionsConstructor.DeclaringType}. Permissions with more than one constructor parameters are not supported");
                }

                try
                {
                    var checkResult = Expression.Parameter(typeof(bool));
                    var checkLambda = Expression.Lambda(Expression.Block(
                        new[] {checkResult},
                        Expression.Assign(checkResult, CreateCheckPermissionCall(permissionExpression)),
                        Expression.IfThen(
                            Expression.IsFalse(checkResult),
                            _checkDeniedHandler(pageType, permissionExpression, appParameterExpression)),
                        checkResult),
                        appParameterExpression).Compile();
                    return Tuple.Create<Type, object>(pageType, checkLambda);
                }
                catch (Exception ex)
                {
                    // todo test
                    throw new Exception(
                        $"Could not create check for page {pageType} and permission {permissionExpression.Type}. Make sure your provided checkDeniedHandler returns correct Expression",
                        ex);
                }
            }

            /// <summary>
            /// Creates a Func that accepts an array of objects from DB and returns true if permission is granted
            /// </summary>
            /// <param name="pageType"></param>
            /// <param name="attributeProvider"></param>
            /// <returns></returns>
            public Func<object[], bool> CreateBoolCheck(Type pageType, ICustomAttributeProvider attributeProvider)
            {
                var permissionsConstructor = GetRequiredPermissionsConstructor(pageType, attributeProvider);
                if (permissionsConstructor == null)
                {
                    return null;
                }

                Expression permissionExpression;
                var objectsParameterExpression = Expression.Parameter(typeof(object[]), "objects");
                var parameterTypes = permissionsConstructor.GetParameters().Select(param => param.ParameterType).ToList();
                switch (parameterTypes.Count)
                {
                    case 0:
                        permissionExpression = Expression.New(permissionsConstructor);
                        break;
                    case 1:
                        var paramType = parameterTypes.First();
                        var pageDataType = GetDataTypeForPage(pageType);
                        if (!pageDataType.IsSubclassOf(paramType) && pageDataType != paramType)
                        {
                            throw new Exception(
                                $"Could not create check for page {pageType} and permission {permissionsConstructor.DeclaringType}. Make sure the page is of type IBound<{paramType}>");
                        }
                        permissionExpression =
                            Expression.Condition(
                                Expression.GreaterThanOrEqual(
                                    Expression.ArrayLength(objectsParameterExpression),
                                    Expression.Constant(1)
                                    ),
                                Expression.New(
                                    permissionsConstructor,
                                    Expression.TypeAs(
                                        Expression.ArrayIndex(objectsParameterExpression, Expression.Constant(0)),
                                        paramType)),
                                // ReSharper disable once AssignNullToNotNullAttribute constructors always have declaring type, silly R#
                                Expression.TypeAs(Expression.Constant(null), permissionsConstructor.DeclaringType));
                        // generates:
                        //  objects.Length == 1 ? new TPermission(objects[0]) : (TPermission)null
                        break;
                    default:
                        throw new Exception(
                            $"Could not create check for page {pageType} and permission {permissionsConstructor.DeclaringType}. Permissions with more than one constructor parameters are not supported");
                }

                return Expression.Lambda<Func<object[], bool>>(
                    CreateCheckPermissionCall(permissionExpression),
                    objectsParameterExpression)
                    .Compile();
            }

            /// <summary>
            /// Creates an Expression that represents permission required by attributes in <see cref="attributesSource"/> that are defined either
            /// on page of type <see cref="pageType"/> or its handlers
            /// </summary>
            /// <param name="pageType"></param>
            /// <param name="attributesSource"></param>
            /// <returns></returns>
            // ReSharper disable once UnusedParameter.Local pageType will be used to support subpage checking
            private ConstructorInfo GetRequiredPermissionsConstructor(Type pageType, ICustomAttributeProvider attributesSource)
            {
                var requirePermissionAttribute = attributesSource
                    .GetCustomAttributes(true)
                    .OfType<RequirePermissionAttribute>()
                    .FirstOrDefault();

                if (requirePermissionAttribute != null)
                {
                    var permissionType = requirePermissionAttribute.RequiredPermission;
                    var permissionConstructors = permissionType.GetConstructors();
                    if (permissionConstructors.Length != 1)
                    {
                        throw new ArgumentException($"Invalid usage of {nameof(RequirePermissionAttribute)} on {attributesSource}: Required permission {permissionType} should have exactly one constructor");
                    }
                    return permissionConstructors[0];
                }

                return null;
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
            public object CreateHandler(MethodInfo originalHandler, Type propertyType, Type pageType, Check checkAction)
            {
                return HandlerTemplate(originalHandler, checkAction, pageType, propertyType);
            }

            /// <summary>
            /// Template for 'handler' function. Performs checkAction and then calls originalHandler (if not null)
            /// </summary>
            /// <param name="originalHandler">original Handler(Input.) method. Ignored if null</param>
            /// <param name="checkAction">underlying type is Func&lt;pageType, bool&gt;</param>
            /// <param name="pageType"></param>
            /// <param name="tType">The type of property that this handler is for</param>
            /// <returns></returns>
            private object HandlerTemplate(MethodInfo originalHandler, Check checkAction, Type pageType, Type tType)
            {
                var jsonParameter = Expression.Parameter(typeof(Json), "json");
                var inputParameter = Expression.Parameter(typeof(Input<>).MakeGenericType(tType), "input");
                var jsonAsTApp = Expression.Convert(jsonParameter, pageType);
                Expression body;
                var checkInvocation = Expression.Invoke(Expression.Constant(checkAction.Item2), jsonAsTApp);
                var cancelEvent = Expression.Call(inputParameter, typeof(Input).GetProperty(nameof(Input.Cancelled)).GetSetMethod(), Expression.Constant(true));
                if (originalHandler != null)
                {
                    var inputType = originalHandler.GetParameters().First().ParameterType;
                    var originalHandlerCall = Expression.Call(jsonAsTApp, originalHandler, Expression.Convert(inputParameter, inputType));
                    body = Expression.IfThenElse(checkInvocation, originalHandlerCall, cancelEvent);
                }
                else
                {
                    body = Expression.IfThen(Expression.IsFalse(checkInvocation), cancelEvent);
                }

                return Expression.Lambda(body, jsonParameter, inputParameter).Compile();
                // reflection based body, for reference:
                //            return (Json pup, Input<T> input) => {
                //                var page = (TApp)pup;
                //                if(checkAction(page))
                //                {
                //                    originalHandler.Invoke(page, new object[] { input });
                //                }
                //                else
                //                {
                //                    input.Cancelled = true;
                //                }
                //            };
                // Or, in case original Handler is null:
                //            return (Json pup, Input<T> input) => {
                //                var page = (TApp)pup;
                //                if(!checkAction(page))
                //                {
                //                    input.Cancelled = true;
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