using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public static class SignInServiceCollectionExtensions
    {
        public static IServiceCollection
            AddSignInManager<TUserSession, TUser>(this IServiceCollection serviceCollection)
            where TUserSession : IUserSession<TUser> where TUser : IUserWithGroups
        {
            serviceCollection.TryAddTransient<IStringSerializer<Claim>, Base64ClaimSerializer>();
            serviceCollection.TryAddTransient<IStringSerializer<ClaimsPrincipal>, Base64ClaimsPrincipalSerializer>();
            serviceCollection.TryAddTransient<IUserClaimsGatherer, UserClaimsGatherer>();
            serviceCollection.TryAddSingleton<ISystemClock>(_ => new SystemClock());
            serviceCollection.TryAddSingleton<ICurrentSessionProvider>(_ => new DefaultCurrentSessionProvider());
            serviceCollection.TryAddTransient<ISignInManager<TUserSession, TUser>, SignInManager<TUserSession, TUser>>();

            return serviceCollection;
        }
    }
}