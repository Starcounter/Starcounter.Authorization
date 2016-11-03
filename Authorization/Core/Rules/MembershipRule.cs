using System.Linq;
using Simplified.Ring2;
using Starcounter.Authorization.Database;

namespace Starcounter.Authorization.Core.Rules
{
    /// <summary>
    /// User will be granted the permission he asks for if he is a member of <see cref="SomebodyGroup"/> associated with the permission.
    /// </summary>
    /// <typeparam name="TPermission"></typeparam>
    public class MembershipRule<TPermission> : ClaimRule<TPermission, PersonClaim> where TPermission : Permission
    {
        public MembershipRule() : base(Predicate)
        {
        }

        private static bool Predicate(PersonClaim claim, TPermission permission)
        {
            var permissionToken = PermissionToken.GetForPermissionOrNull(permission);
            if (permissionToken == null)
            {
                return false;
            }
            return Db.SQL<SomebodyGroup>($"select a.\"{nameof(PermissionSomebodyGroup.Group)}\" from {typeof(PermissionSomebodyGroup).FullName} a where a.\"{nameof(PermissionSomebodyGroup.Permission)}\" = ?", permissionToken)
                .Any(group => @group.GetAllMembers().Contains(claim.Person));
        }
    }
}