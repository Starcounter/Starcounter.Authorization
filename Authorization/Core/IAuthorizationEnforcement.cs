namespace Starcounter.Authorization.Core
{
    public interface IAuthorizationEnforcement
    {
        bool CheckPermission<TPermission>(TPermission permission) where TPermission : Permission;
    }
}