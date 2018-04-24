using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public class CurrentUserProvider<TUserAuthenticationTicket, TUser> : ICurrentUserProvider<TUser>
        where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>
        where TUser : class, IUser
    {
        private readonly IAuthenticationTicketProvider<TUserAuthenticationTicket> _authenticationTicketProvider;

        public CurrentUserProvider(IAuthenticationTicketProvider<TUserAuthenticationTicket> authenticationTicketProvider)
        {
            _authenticationTicketProvider = authenticationTicketProvider;
        }
        public TUser GetCurrentUser()
        {
            var authenticationTicket = _authenticationTicketProvider.GetCurrentAuthenticationTicket();
            return authenticationTicket?.User;
        }
    }
}