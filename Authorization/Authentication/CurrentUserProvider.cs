using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    internal class CurrentUserProvider<TUserAuthenticationTicket, TUser> : ICurrentUserProvider<TUser>
        where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>
        where TUser : class, IMinimalUser
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