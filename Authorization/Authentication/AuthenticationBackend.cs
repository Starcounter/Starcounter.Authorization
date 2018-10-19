using System.Security.Claims;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    /// <summary>
    /// Provides <see cref="ClaimsPrincipal"/> that is either anonymous or authenticated, but without any claims.
    /// To get <see cref="ClaimsPrincipal"/> with claims, use <see cref="UserAuthenticationBackend{TAuthenticationTicket,TUser}"/>
    /// </summary>
    /// <typeparam name="TAuthenticationTicket"></typeparam>
    internal class AuthenticationBackend<TAuthenticationTicket> : IAuthenticationBackend
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;

        public AuthenticationBackend(IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService)
        {
            _authenticationTicketService = authenticationTicketService;
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var authenticationTicket = _authenticationTicketService.GetCurrentAuthenticationTicket();
            if (authenticationTicket == null)
            {
                return new ClaimsPrincipal();
            }

            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], AuthenticationTypes.Starcounter));
        }
    }
}