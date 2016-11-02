using System;
using System.Linq;
using Simplified.Ring2;
using Starcounter.Authorization.Database;

namespace Starcounter.Authorization.Core.Rules
{
    public static class AuthorizationConfiguratorExtensions
    {
        /// <summary>
        /// User possesing a TClaim will be granted TPermission if <paramref name="predicate"/> evaluates to true.
        /// If user has more than one claims of type TClaim, he will be granted the permission if any of the claims passes the predicate.
        /// </summary>
        /// <typeparam name="TPermission"></typeparam>
        /// <typeparam name="TClaim"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="predicate"></param>
        public static void AddClaimRule<TPermission, TClaim>(this IAuthorizationConfigurator configurator, Func<TClaim, TPermission, bool> predicate)
            where TPermission : Permission where TClaim : Claim
        {
            configurator.AddRule<TPermission>((claims, permission) => claims.OfType<TClaim>().Any(claim => predicate(claim, permission)));
        }

        /// <summary>
        /// Any user possesing claim of type TClaim will be granted any TPermission he asks for
        /// </summary>
        /// <typeparam name="TPermission"></typeparam>
        /// <typeparam name="TClaim"></typeparam>
        /// <param name="configurator"></param>
        public static void RequireClaim<TPermission, TClaim>(this IAuthorizationConfigurator configurator) where TPermission : Permission where TClaim : Claim
        {
            configurator.AddClaimRule<TPermission, TClaim>((claim, permission) => true);
        }

        /// <summary>
        /// User will be granted the permission he asks for if he is a member of <see cref="SomebodyGroup"/> associated with the permission.
        /// </summary>
        /// <typeparam name="TPermission"></typeparam>
        /// <param name="configurator"></param>
        public static void RequireMembership<TPermission>(this IAuthorizationConfigurator configurator)
            where TPermission : Permission
        {
            configurator.AddClaimRule<TPermission, PersonClaim>((claim, permission) => {
                var permissionToken = PermissionToken.GetForPermissionOrNull(permission);
                if (permissionToken == null)
                {
                    return false;
                }
                return Db.SQL<SomebodyGroup>(
                    $"select a.\"{nameof(PermissionSomebodyGroup.Group)}\" from {typeof(PermissionSomebodyGroup).FullName} a where a.\"{nameof(PermissionSomebodyGroup.Permission)}\" = ?", permissionToken)
                    .Any(group => group.GetAllMembers().Contains(claim.Person));
            });
        }
    }
}