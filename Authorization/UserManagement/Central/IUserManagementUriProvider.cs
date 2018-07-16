using System;

namespace Starcounter.Authorization.UserManagement.Central
{
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public interface IUserManagementUriProvider<in TUser>
        where TUser : IMinimalUser
    {
        string EditUserUriTemplate { get; }

        /// <summary>
        /// Returns a URI that can be passed to <see cref="Self.GET(string)"/> to obtain blended "edit user" partials from all applications
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string CreateEditUserUri(TUser user);
    }
}