using System;
using System.Linq;

namespace Starcounter.Authorization.Core.Rules
{
    public static class AuthorizationConfiguratorExtensions
    {
        public static void AddClaimRule<TPermission, TClaim>(this IAuthorizationConfigurator configurator, Func<TClaim, TPermission, bool> predicate)
            where TPermission : Permission where TClaim : Claim
        {
            configurator.AddRule<TPermission>((claims, permission) => {
                var typedClaim = claims.OfType<TClaim>().FirstOrDefault();
                return typedClaim != null && predicate(typedClaim, permission);
            });
        }

        public static void RequireClaim<TPermission, TClaim>(this IAuthorizationConfigurator configurator) where TPermission : Permission where TClaim : Claim
        {
            configurator.AddClaimRule<TPermission, TClaim>((claim, permission) => true);
        }
    }
}