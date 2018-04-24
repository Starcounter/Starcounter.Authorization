using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public static class SignInServiceCollectionExtensions
    {
        public static IServiceCollection
            AddSignInManager<TAuthenticationTicket, TUser>(this IServiceCollection serviceCollection)
            where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>
            where TUser : IUserWithGroups
        {
            serviceCollection.TryAddTransient<IClaimDbConverter, ClaimDbConverter>();
            serviceCollection.TryAddTransient<IClaimsPrincipalSerializer, Base64ClaimsPrincipalSerializer>();
            serviceCollection.TryAddTransient<IUserClaimsGatherer, UserClaimsGatherer>();
            serviceCollection.TryAddSingleton<ISystemClock>(_ => new SystemClock());
            serviceCollection.TryAddSingleton<ICurrentSessionProvider>(_ => new DefaultCurrentSessionProvider());
            serviceCollection.TryAddTransient<ISignInManager<TAuthenticationTicket, TUser>, SignInManager<TAuthenticationTicket, TUser>>();

            return serviceCollection;
        }
    }
}