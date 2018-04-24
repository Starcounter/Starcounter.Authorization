using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication<TAuthenticationTicket>(this IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<IAuthenticationBackend, AuthenticationBackend<TAuthenticationTicket>>();
            services.TryAddTransient<IStringSerializer<ClaimsPrincipal>, Base64ClaimsPrincipalSerializer>();
            AddAuthenticationTicketProvider<TAuthenticationTicket>(services);
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

        public static IServiceCollection AddFakeAuthentication(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthenticationBackend, AlwaysAdminAuthBackend>();
            return services;
        }
        private static void AddAuthenticationTicketProvider<TAuthenticationTicket>(IServiceCollection services)
            where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        {
            services.TryAddTransient<ISystemClock, SystemClock>();
            services.TryAddTransient<ICurrentSessionProvider, DefaultCurrentSessionProvider>();
            services.TryAddTransient<IScAuthenticationTicketRepository<TAuthenticationTicket>, ScAuthenticationTicketRepository<TAuthenticationTicket>>();
            services.TryAddTransient<IAuthenticationTicketProvider<TAuthenticationTicket>, AuthenticationTicketProvider<TAuthenticationTicket>>();
        }
    }
}