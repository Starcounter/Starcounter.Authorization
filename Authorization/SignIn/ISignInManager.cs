using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    public interface ISignInManager<in TUserSession, in TUser> where TUserSession : IUserSession<TUser> where TUser : IUserWithGroups
    {
        void SignIn(TUser user, TUserSession session);
    }
}