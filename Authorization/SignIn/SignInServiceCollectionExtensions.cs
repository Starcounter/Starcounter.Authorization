using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public static class SignInServiceCollectionExtensions
    {
        public static IServiceCollection
            AddSignInManager<TAuthenticationTicket, TUser>(this IServiceCollection serviceCollection)
            where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>, new()
            where TUser : IUser
        {
            serviceCollection.TryAddTransient<IClaimDbConverter, ClaimDbConverter>();
            serviceCollection.TryAddTransient<IUserClaimsGatherer, UserClaimsGatherer>();
            serviceCollection.TryAddSingleton<ISystemClock>(_ => new SystemClock());
            serviceCollection.TryAddSingleton<ICurrentSessionProvider>(_ => new DefaultCurrentSessionProvider());
            serviceCollection.TryAddTransient<IScAuthenticationTicketRepository<TAuthenticationTicket>, ScAuthenticationTicketRepository<TAuthenticationTicket>>();
            serviceCollection.TryAddTransient<ISignInManager<TUser>, SignInManager<TAuthenticationTicket, TUser>>();

            return serviceCollection;
        }
    }
}