using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    internal class CurrentUserProvider<TUserAuthenticationTicket, TUser> : ICurrentUserProvider<TUser>
        where TUserAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>
        where TUser : class, IMinimalUser
    {
        private readonly IAuthenticationTicketService<TUserAuthenticationTicket> _authenticationTicketService;

        public CurrentUserProvider(IAuthenticationTicketService<TUserAuthenticationTicket> authenticationTicketService)
        {
            _authenticationTicketService = authenticationTicketService;
        }
        public TUser GetCurrentUser()
        {
            var authenticationTicket = _authenticationTicketService.GetCurrentAuthenticationTicket();
            return authenticationTicket?.User;
        }
    }
}