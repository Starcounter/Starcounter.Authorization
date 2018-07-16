using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    public interface ICurrentUserProvider<TUser> where TUser : IMinimalUser
    {
        TUser GetCurrentUser();
    }
}