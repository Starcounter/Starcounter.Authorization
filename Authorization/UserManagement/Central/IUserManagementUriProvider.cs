using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.UserManagement.Central
{
    public interface IUserManagementUriProvider<in TUser>
        where TUser : IUser
    {
        string EditUserUriTemplate { get; }
        string CreateEditUserUri(TUser claimDb);
    }
}