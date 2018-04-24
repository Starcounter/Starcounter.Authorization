using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    public interface ISignInManager<in TAuthenticationTicket, in TUser> 
        where TAuthenticationTicket : IScUserAuthenticationTicket<TUser> 
        where TUser : IUserWithGroups
    {
        void SignIn(TUser user, TAuthenticationTicket authenticationTicket);
    }
}