using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface ICurrentUserProvider<TUser> where TUser : IUser
    {
        TUser GetCurrentUser();
    }
}