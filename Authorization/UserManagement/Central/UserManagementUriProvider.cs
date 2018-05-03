using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.UserManagement.Central
{
    public class UserManagementUriProvider<TUser> : IUserManagementUriProvider<TUser>
        where TUser : IUser
    {
        public string EditUserUriTemplate => "/UserAdmin/Authorization.UserManagement.Master/{?}";
        public string CreateEditUserUri(TUser user)
        {
            return EditUserUriTemplate.Replace("{?}", user.GetObjectID());
        }
    }
}