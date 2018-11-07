using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Middleware;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Settings;
using Starcounter.Authorization.SignIn;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;
using AuthorizationOptions = Starcounter.Authorization.Settings.AuthorizationOptions;

namespace Starcounter.Authorization
{
    public static class AuthenticationServiceCollectionExtensions
    {
        /// <summary>
        /// Add full starcounter authorization services to the service collection.
        /// Claims associated with the user and its groups will be retrieved every time
        /// any authorization rules will be evaluated.
        /// </summary>
        /// <typeparam name="TAuthorizationSettings">Authentication settings type defined in application model. Should implement <see cref="IAuthorizationSettings"/> and nothing more</typeparam>
        /// <typeparam name="TAuthenticationTicket">Authentication ticket type defined in application model. Should implement <see cref="IScUserAuthenticationTicket{TUser}"/> and nothing more</typeparam>
        /// <typeparam name="TUser">Application-specific user type</typeparam>
        /// <param name="services">Service collection to add to. This will be modified, not copied, by this method.</param>
        /// <param name="configure">Configuration of authorization rules. This will be passed to <see cref="AuthorizationServiceCollectionExtensions.AddAuthorization(IServiceCollection)"/>.</param>
        /// <returns>Original service collection, with new services added.</returns>
        public static IServiceCollection AddStarcounterAuthorization<TAuthorizationSettings, TAuthenticationTicket, TUser>(
            this IServiceCollection services,
            Action<Microsoft.AspNetCore.Authorization.AuthorizationOptions> configure = null)
            where TAuthorizationSettings: class, IAuthorizationSettings, new()
            where TAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>, new()
            where TUser : class, IUser
        {
            // it's important this comes first, because it enables the use of IOptions<AuthorizationOptions>
            AddAuthorizationSettings<TAuthorizationSettings>(services);
            AddCurrentUserProvider<TAuthenticationTicket, TUser>(services);
            AddUserAuthenticationBackend<TAuthenticationTicket, TUser>(services);
            AddAuthenticationTicketProvider<TAuthenticationTicket, TUser>(services);
            AddCookieSignInMiddleware<TAuthenticationTicket>(services);
            AddSecurityMiddleware<TAuthenticationTicket>(services);
            if (configure != null)
            {
                services.AddAuthorization(configure);
            }
            else
            {
                services.AddAuthorization();
            }
            return services;
        }

        /// <summary>
        /// Add <see cref="ICurrentUserProvider{TUser}"/> to service collection. 
        /// </summary>
        /// <typeparam name="TUserAuthenticationTicket">Authentication ticket type defined in application model. Should implement <see cref="IScUserAuthenticationTicket{TUser}"/> and nothing more</typeparam>
        /// <typeparam name="TUser">Application-specific user type</typeparam>
        /// <param name="services">Service collection to add to. This will be modified, not copied, by this method.</param>
        /// <returns>Original service collection, with new services added.</returns>
        public static IServiceCollection
            AddCurrentUserProvider<TUserAuthenticationTicket, TUser>(this IServiceCollection services)
            where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>, new()
            where TUser : class, IUser
        {
            services.TryAddTransient<ICurrentUserProvider<TUser>, CurrentUserProvider<TUserAuthenticationTicket, TUser>>();
            AddAuthenticationTicketProvider<TUserAuthenticationTicket, TUser>(services);
            return services;
        }

        /// <summary>
        /// Registers a claim type this application will use. The claim template for this type will be created if required.
        /// </summary>
        /// <typeparam name="TClaimTemplate"></typeparam>
        /// <param name="services">Service collection to add to. This will be modified, not copied, by this method.</param>
        /// <param name="claimType">The string representation of claim type to add. It should be globally unique.
        /// It's recommended to use a URI associated with the registering app as a prefix. This string should never change after it is registered for the first time.</param>
        /// <returns>Original service collection, with new services added.</returns>
        public static IServiceCollection AddClaimType<TClaimTemplate>(this IServiceCollection services,
            string claimType)
            where TClaimTemplate : IClaimTemplate, new()
        {
            services.TryAddTransient<IClaimDbConverter, ClaimDbConverter>();
            services.AddTransient<IStartupFilter>(provider => ActivatorUtilities.CreateInstance<ClaimCreatorStartupFilter<TClaimTemplate>>(provider, claimType));
            return services;
        }

        /// <summary>
        /// Add support for using authorization settings from the database.
        /// </summary>
        /// <typeparam name="TAuthorizationSettings">Authentication settings type defined in application model. Should implement <see cref="IAuthorizationSettings"/> and nothing more</typeparam>
        /// <param name="services">Service collection to add to. This will be modified, not copied, by this method.</param>
        /// <returns>Original service collection, with new services added.</returns>
        private static IServiceCollection AddAuthorizationSettings<TAuthorizationSettings>(
            this IServiceCollection services)
            where TAuthorizationSettings : class, IAuthorizationSettings, new()
        {
            services.TryAddTransient<IOptions<AuthorizationOptions>, OptionsProvider<TAuthorizationSettings>>();
            services.TryAddTransient<ISettingsService<TAuthorizationSettings>, SettingsService<TAuthorizationSettings>>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, EnsureSettingsStartupFilter<TAuthorizationSettings>>());
            return services;
        }

        private static void AddUserAuthenticationBackend<TUserAuthenticationTicket, TUser>(IServiceCollection services)
            where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>, new()
            where TUser : class, IUser
        {
            services.TryAddTransient<IAuthenticationBackend, UserAuthenticationBackend<TUserAuthenticationTicket, TUser>>();
            services.TryAddTransient<IUserClaimsGatherer, UserClaimsGatherer>();
            services.TryAddTransient<IClaimDbConverter, ClaimDbConverter>();
        }

        internal static IServiceCollection AddSecurityMiddleware<TAuthenticationTicket>(this IServiceCollection services) 
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
            services.TryAddTransient<IPostConfigureOptions<SecurityMiddlewareOptions>, DefaultSecurityMiddlewareOptions>();
            return services;
        }

        internal static void AddAuthenticationTicketProvider<TAuthenticationTicket, TUser>(this IServiceCollection services)
            where TAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>, new()
            where TUser : class, IUser
        {
            services.TryAddTransient<ISystemClock, SystemClock>();
            services.TryAddTransient<ITransactionFactory, StarcounterTransactionFactory>();
            services.TryAddTransient<ICurrentSessionProvider, DefaultCurrentSessionProvider>();
            services.TryAddTransient<IScAuthenticationTicketRepository<TAuthenticationTicket>, ScAuthenticationTicketRepository<TAuthenticationTicket>>();
            services.TryAddTransient<IAuthenticationTicketService<TAuthenticationTicket>, AuthenticationTicketService<TAuthenticationTicket, TUser>>();
            services.TryAddTransient<ISecureRandom, SecureRandom>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, CleanupStartupFilter<TAuthenticationTicket>>());
        }

        private static void AddCookieSignInMiddleware<TAuthenticationTicket>(IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<IAuthCookieService, AuthCookieService<TAuthenticationTicket>>();
            services.TryAddTransient<ISecureRandom, SecureRandom>();
            services.TryAddTransient<ITransactionFactory, StarcounterTransactionFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPageMiddleware, EnsureSessionMiddleware>());
        }
    }
}