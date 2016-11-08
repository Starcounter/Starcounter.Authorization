using System;
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
        private readonly IAuthorizationEnforcement _authorizationEnforcement;
        private readonly List<Type> _enhancedTypes = new List<Type>();

        public PageSecurity(IAuthorizationEnforcement authorizationEnforcement)
        {
            _authorizationEnforcement = authorizationEnforcement;
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
            return CheckNonData(pageType) && CheckData(pageType, data);
        }

        private bool CheckNonData(Type pageType)
        {
            var requirePermissionAttribute = pageType.GetCustomAttribute<RequirePermissionAttribute>();
            if (requirePermissionAttribute == null)
            {
                return true;
            }

            var permissionType = requirePermissionAttribute.RequiredPermission;
            object permission;
            try
            {
                permission = permissionType.GetConstructor(new Type[0]).Invoke(new object[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not find suitable ctor for permission {permissionType}. Make sure it has a default ctor", ex);
            }

            return (bool) InvokePrivateGenericMethod(nameof(CheckPermission), new[] {permissionType}, permission);
        }

        private bool CheckData(Type pageType, object data)
        {
            var requirePermissionAttribute = pageType.GetCustomAttribute<RequirePermissionDataAttribute>();
            if (requirePermissionAttribute == null)
            {
                return true;
            }
            if (data == null)
            {
                return false;
            }

            var permissionType = requirePermissionAttribute.RequiredPermission;
            object permission;
            try
            {
                permission = permissionType.GetConstructor(new []{data.GetType()}).Invoke(new[] {data});
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not find suitable ctor for permission {permissionType}. Make sure it has a ctor accepting {data.GetType()}", ex);
            }
            return (bool) InvokePrivateGenericMethod(nameof(CheckPermission), new[] {permissionType}, permission);
        }

        private bool CheckPermission<T>(T permission) where T : Permission
        {
            return _authorizationEnforcement.CheckPermission(permission);
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
                        ? RecreateCreateInputEvent(originalHandlerMethod, propertyType)
                        : InvokePrivateGenericMethod(nameof(CreateMethodA), new[] { propertyType });
                    property.GetType().GetMethod(nameof(Property<int>.AddHandler)).Invoke(
                        property,
                        new[]
                        {
                            createInputEvent,
                            CreateHandler(originalHandlerMethod, propertyType, pageType, checkAction)
                        });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not generate handler for property {tuple.Item2}", ex);
                }
            }
        }

        private Tuple<MethodInfo, Template, RequirePermissionAttribute> VerifyRequirePermissionAttribute(Tuple<MethodInfo, Template, RequirePermissionAttribute> tpl)
        {
            if (tpl.Item3.RequiredPermission.GetConstructor(new Type[0]) == null)
            {
                throw new ArgumentException($"Invalid usage of RequirePermission on method {tpl.Item1}: Required permission {tpl.Item3.RequiredPermission} has no default ctor");
            }

            return tpl;
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
            object pageCheck = CreatePageCheck(pageType);

            var pageProperties = pageDefaultTemplate.Properties;
            var existingHandlers = pageType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "Handle")
                .Select(method => Tuple.Create(method, FindPropertyByHandlerMethod(method, pageProperties)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();

            var existingHandlersList = existingHandlers.ToList();
            var handlersWithRequiredDataAttribute = existingHandlersList
                .Select(
                    tpl => Tuple.Create(tpl.Item1, tpl.Item2, tpl.Item1.GetCustomAttribute<RequirePermissionDataAttribute>()))
                .Where(tuple => tuple.Item3 != null)
                .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, CreateDataCheck(pageType, tpl.Item3.RequiredPermission)))
                .ToList();

            var handlersWithRequiredAttribute = existingHandlersList
                .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, tpl.Item1.GetCustomAttribute<RequirePermissionAttribute>()))
                .Where(tuple => tuple.Item3 != null)
                .Select(VerifyRequirePermissionAttribute)
                .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, CreateNonDataCheck(pageType, tpl.Item3.RequiredPermission)))
                .ToList();

            var handlersWithBothAttributes = handlersWithRequiredAttribute.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2))
                .Intersect(handlersWithRequiredDataAttribute.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2)))
                .ToList();
            if (handlersWithBothAttributes.Any())
            {
                var handlersNames = string.Join(",", handlersWithBothAttributes.Select(tuple => tuple.Item1.ToString()));
                throw new Exception(
                    $"Each handler can have only one of attributes: {nameof(RequirePermissionDataAttribute)} or {nameof(RequirePermissionAttribute)}. Violating handlers: {handlersNames}");
            }

            var allHandlersTasks = handlersWithRequiredDataAttribute
                .Concat(handlersWithRequiredAttribute);
            if (pageCheck != null)
            {
                var handlersWithoutAttributes = existingHandlersList
                    .Except(handlersWithRequiredAttribute.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2)))
                    .Except(handlersWithRequiredDataAttribute.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2)))
                    .Select(tpl => Tuple.Create(tpl.Item1, tpl.Item2, pageCheck))
                    .ToList();

                var propertiesWithoutHandlers = pageProperties
                    .Except(existingHandlers.Select(tuple => tuple.Item2))
                    .Select(template => Tuple.Create<MethodInfo, Template, object>(null, template, pageCheck));

                allHandlersTasks = allHandlersTasks
                    .Concat(handlersWithoutAttributes)
/*                    .Concat(propertiesWithoutHandlers)*/;
            }
            return allHandlersTasks;
        }

        /// <summary>
        /// Creates the actual Action&lt;pageType&gt; that checks permissions for default actions in page
        /// as well as loading the page
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns></returns>
        private object CreatePageCheck(Type pageType)
        {
            var pageRequireDataAttribute = pageType.GetCustomAttribute<RequirePermissionDataAttribute>();
            var pageRequireAttribute = pageType.GetCustomAttribute<RequirePermissionAttribute>();
            if (pageRequireAttribute != null && pageRequireDataAttribute != null)
            {
                throw new Exception(
                    $"There should be only one of attributes: {nameof(RequirePermissionDataAttribute)} or {nameof(RequirePermissionAttribute)} in page class {pageType}");
            }
            if (pageRequireDataAttribute != null)
            {
                return CreateDataCheck(pageType, pageRequireDataAttribute.RequiredPermission);
            }
            if (pageRequireAttribute != null)
            {
                return CreateNonDataCheck(pageType, pageRequireAttribute.RequiredPermission);
            }
            // TODO parent page
            return null;
        }

        private object CreateDataCheck(Type pageType, Type permissionType)
        {
            Type dataType;
            try
            {
                dataType = pageType.GetInterface($"{nameof(IBound<int>)}`1").GetGenericArguments().First();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not determine Data type for page class {pageType}. Please make sure you marked it with IBound interface", ex);
            }
            try
            {
                return InvokePrivateGenericMethod(nameof(DataCheckTemplate), new[] { pageType, permissionType, dataType });
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not generate Data-based authorization check for page class {pageType}", ex);
            }
        }

        private object CreateNonDataCheck(Type pageType, Type permissionType)
        {
            try
            {
                return InvokePrivateGenericMethod(nameof(NonDataCheckTemplate), new[] { pageType, permissionType });
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not generate authorization check for page class {pageType}, make sure that permission {permissionType} extends Permission and has a default ctor", ex);
            }
        }

        private object InvokePrivateGenericMethod(string name, Type[] typeParameter, params object[] arguments)
        {
            return typeof(PageSecurity)
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeParameter)
                .Invoke(this, arguments);
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

        private Func<Json, Property<T>, T, Input<T>> CreateMethodA<T>()
        {
            return (Json pup, Property<T> prop, T value) => new Input<T>()
            {
                Value = value
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalHandler"></param>
        /// <param name="propertyType"></param>
        /// <param name="pageType"></param>
        /// <param name="checkAction">This should be of type Action&lt;pageType&gt;</param>
        /// <returns></returns>
        private object CreateHandler(MethodInfo originalHandler, Type propertyType, Type pageType, object checkAction)
        {
            return InvokePrivateGenericMethod(nameof(HandlerTemplate), new[] { propertyType, pageType }, originalHandler,
                checkAction);
        }

        private object RecreateCreateInputEvent(MethodInfo originalHandler, Type propertyType)
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
            return InvokePrivateGenericMethod(nameof(CreateInputEventTemplate),
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

        private Action<TApp> DataCheckTemplate<TApp, TPermission, TData>()
            where TPermission : Permission 
            where TApp : Json
        {
            var dataType = typeof(TData);
            var permissionCtor = typeof(TPermission).GetConstructor(new[] {dataType});
            if (permissionCtor == null)
            {
                throw new Exception($"Could not find suitable ctor for type {typeof(TPermission)}. Make sure it has ctor that accepts {dataType} as argument");
            }
            var appParameter = Expression.Parameter(typeof(TApp), "app");
            var createPermission = Expression.New(permissionCtor, Expression.MakeMemberAccess(appParameter, typeof(TApp).GetProperty("Data", dataType)));
            var authEnforcement = Expression.Constant(_authorizationEnforcement);
            var tryPermissionOrThrowMethod = typeof(AuthorizationEnforcementExtensions)
                .GetMethod(nameof(AuthorizationEnforcementExtensions.TryPermissionOrThrow))
                .MakeGenericMethod(typeof(TPermission));
            return Expression.Lambda<Action<TApp>>(
                    Expression.Call(null, tryPermissionOrThrowMethod, authEnforcement, createPermission),
                    appParameter).Compile();
            // generated code should look like this:
            // return app => {
            //    _authorizationEnforcement.TryPermissionOrThrow(new TPermission((TData)app.Data));
            // };
        }

        private Action<TApp> NonDataCheckTemplate<TApp, TPermission>() where TPermission : Permission, new() where TApp : Json
        {
            return app => {
                _authorizationEnforcement.TryPermissionOrThrow(new TPermission());
            };
        }
    }
}