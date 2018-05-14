using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Middleware;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;

namespace Starcounter.Authorization.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication<TAuthenticationTicket>(this IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<IAuthenticationBackend, AuthenticationBackend<TAuthenticationTicket>>();
            services.TryAddTransient<IClaimsPrincipalSerializer, Base64ClaimsPrincipalSerializer>();
            AddAuthenticationTicketProvider<TAuthenticationTicket>(services);
            services.TryAddTransient<IAuthorizationEnforcement, AuthorizationEnforcement>();
            AddCookieSignInMiddleware<TAuthenticationTicket>(services);
            AddSecurityMiddleware<TAuthenticationTicket>(services);
            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection
            AddUserConfiguration<TUserAuthenticationTicket, TUser>(this IServiceCollection services)
            where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>, new()
            where TUser : class, IUser
        {
            services.TryAddTransient<ICurrentUserProvider<TUser>, CurrentUserProvider<TUserAuthenticationTicket, TUser>>();
            AddAuthenticationTicketProvider<TUserAuthenticationTicket>(services);
            return services;
        }

        private static void AddAuthenticationTicketProvider<TAuthenticationTicket>(IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<ISystemClock, SystemClock>();
            services.TryAddTransient<ITransactionFactory, StarcounterTransactionFactory>();
            services.TryAddTransient<ICurrentSessionProvider, DefaultCurrentSessionProvider>();
            services.TryAddTransient<IScAuthenticationTicketRepository<TAuthenticationTicket>, ScAuthenticationTicketRepository<TAuthenticationTicket>>();
            services.TryAddTransient<IAuthenticationTicketProvider<TAuthenticationTicket>, AuthenticationTicketProvider<TAuthenticationTicket>>();
        }

        private static void AddCookieSignInMiddleware<TAuthenticationTicket>(IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<IAuthCookieService, AuthCookieService<TAuthenticationTicket>>();
            services.TryAddTransient<ISecureRandom, SecureRandom>();
            services.TryAddTransient<ITransactionFactory, StarcounterTransactionFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPageMiddleware, CookieSignInMiddleware<TAuthenticationTicket>>());
        }

        public static IServiceCollection AddSecurityMiddleware<TAuthenticationTicket>(this IServiceCollection services) 
            where TAuthenticationTicket : class, IScAuthenticationTicket
        {
            services.TryAddSingleton<IAuthorizationEnforcement, AuthorizationEnforcement>();
            services.TryAddSingleton<PageSecurity.PageSecurity>();
            services.TryAddSingleton<CheckersCreator>();
            services.TryAddSingleton<IAttributeRequirementsResolver, AttributeRequirementsResolver>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPageMiddleware, SecurityMiddleware>());
            services.TryAddTransient<ISignOutService, SignOutService<TAuthenticationTicket>>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, AuthenticationStartupFilter>());
            services.TryAddTransient<IAuthenticationUriProvider, AuthenticationUriProvider>();

            return services;
        }

        public static IServiceCollection AddSecurityMiddleware<TAuthenticationTicket>(
            this IServiceCollection services,
            Action<SecurityMiddlewareOptions> configure) 
            where TAuthenticationTicket : class, IScAuthenticationTicket
        {
            services.Configure(configure);
            return AddSecurityMiddleware<TAuthenticationTicket>(services);
        }

    }
}