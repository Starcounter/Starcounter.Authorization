using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthentication<TSession>(this IServiceCollection services) where TSession: class, ISession, new()
        {
            services.TryAddTransient<IAuthenticationBackend, AuthenticationBackend<TSession>>();
            services.TryAddTransient<IStringSerializer<ClaimsPrincipal>, Base64ClaimsPrincipalSerializer>();
            services.TryAddTransient<ISystemClock, SystemClock>();
            services.TryAddTransient<ICurrentSessionProvider, DefaultCurrentSessionProvider>();
            services.TryAddTransient<ISessionRepository<TSession>, StarcounterSessionRepository<TSession>>();
            services.TryAddTransient<ICurrentSessionRetriever<TSession>, CurrentSessionRetriever<TSession>>();
            return services;
        }

        public static IServiceCollection AddUserConfiguration<TUserSession, TUser>(this IServiceCollection services) where TUserSession : class, IUserSession<TUser>, new() where TUser : IUser
        {
            services.TryAddTransient<ICurrentUserProvider<TUser>, CurrentUserProvider<TUserSession, TUser>>();
            services.TryAddTransient<ISystemClock, SystemClock>();
            services.TryAddTransient<ICurrentSessionProvider, DefaultCurrentSessionProvider>();
            services.TryAddTransient<ISessionRepository<TUserSession>, StarcounterSessionRepository<TUserSession>>();
            services.TryAddTransient<ICurrentSessionRetriever<TUserSession>, CurrentSessionRetriever<TUserSession>>();
            return services;
        }

        public static IServiceCollection AddFakeAuthentication(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthenticationBackend, AlwaysAdminAuthBackend>();
            return services;
        }
    }
}