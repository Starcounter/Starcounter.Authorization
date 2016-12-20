using Starcounter.Authorization.Core;
using Starcounter.Authorization.Database;

namespace Starcounter.Authorization.Helper
{
    public static class PermissionHelper
    {
        public static QueryResultRows<PermissionSomebodyGroup> GetAllPsgForPermission(Permission permission)
        {
            return GetAllPsgForPermission(PermissionToken.GetForPermissionOrNull(permission));
        }

        public static QueryResultRows<PermissionSomebodyGroup> GetAllPsgForPermission(PermissionToken permissionToken)
        {
            return Db.SQL<PermissionSomebodyGroup>(
                $"select a from {typeof(PermissionSomebodyGroup).FullName} a where a.{nameof(PermissionSomebodyGroup.Permission)} = ?",
                permissionToken);
        }
    }
}