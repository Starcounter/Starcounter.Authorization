using System.Security.Claims;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.SignIn;

namespace Starcounter.Authorization.Authentication
{
    /// <summary>
    /// Provides <see cref="ClaimsPrincipal"/> that is either anonymous or contains claims held by authenticated user.
    /// To avoid specifying TUser, use <see cref="AuthenticationBackend{TAuthenticationTicket}"/>
    /// </summary>
    /// <typeparam name="TAuthenticationTicket"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    internal class UserAuthenticationBackend<TAuthenticationTicket, TUser> : IAuthenticationBackend
        where TAuthenticationTicket : class, IScUserAuthenticationTicket<TUser>
        where TUser : IUser
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly IUserClaimsGatherer _claimsGatherer;

        public UserAuthenticationBackend(IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            IUserClaimsGatherer claimsGatherer)
        {
            _authenticationTicketService = authenticationTicketService;
            _claimsGatherer = claimsGatherer;
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var authenticationTicket = _authenticationTicketService.GetCurrentAuthenticationTicket();
            if (authenticationTicket == null || authenticationTicket.User == null)
            {
                return new ClaimsPrincipal();
            }

            return new ClaimsPrincipal(new ClaimsIdentity(_claimsGatherer.Gather(authenticationTicket.User), AuthenticationTypes.Starcounter));
        }
    }
}