using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Middleware;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.PageSecurity
{
    public static class SecurityMiddlewareServiceCollectionExtensions
    {
        public static IServiceCollection AddSecurityMiddleware(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthorizationEnforcement, AuthorizationEnforcement>();
            services.TryAddSingleton<PageSecurity>();
            services.TryAddSingleton<CheckersCreator>();
            services.TryAddSingleton<IAttributeRequirementsResolver, AttributeRequirementsResolver>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPageMiddleware, SecurityMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, AuthenticationStartupFilter>());
            services.TryAddTransient<IAuthenticationUriProvider, AuthenticationUriProvider>();

            return services;
        }

        public static IServiceCollection AddSecurityMiddleware(this IServiceCollection services,
            Action<SecurityMiddlewareOptions> configure)
        {
            services.Configure(configure);
            return AddSecurityMiddleware(services);
        }

    }
}