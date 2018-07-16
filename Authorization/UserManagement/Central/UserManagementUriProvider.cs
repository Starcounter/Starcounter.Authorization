using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.UserManagement.Central
{
    internal class UserManagementUriProvider<TUser> : IUserManagementUriProvider<TUser>
        where TUser : IMinimalUser
    {
        public string EditUserUriTemplate => $"/{Application.Current.Name}/Authorization.UserManagement.Master/{{?}}";
        public string CreateEditUserUri(TUser user)
        {
            return EditUserUriTemplate.Replace("{?}", user.GetObjectID());
        }
    }
}