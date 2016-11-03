namespace Starcounter.Authorization.Core
{
    public static class AuthorizationEnforcementExtensions
    {
        public static void TryPermissionOrThrow<TPermission>(this IAuthorizationEnforcement enforcement, TPermission permission) where TPermission : Permission
        {
            if (!enforcement.CheckPermission(permission))
            {
                throw new UnauthorizedException();
            }
        }
    }
}