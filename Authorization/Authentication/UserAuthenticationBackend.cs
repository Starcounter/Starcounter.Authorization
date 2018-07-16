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
        private readonly IAuthenticationTicketProvider<TAuthenticationTicket> _authenticationTicketProvider;
        private readonly IUserClaimsGatherer _claimsGatherer;

        public UserAuthenticationBackend(IAuthenticationTicketProvider<TAuthenticationTicket> authenticationTicketProvider,
            IUserClaimsGatherer claimsGatherer)
        {
            _authenticationTicketProvider = authenticationTicketProvider;
            _claimsGatherer = claimsGatherer;
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var authenticationTicket = _authenticationTicketProvider.GetCurrentAuthenticationTicket();
            if (authenticationTicket == null)
            {
                return new ClaimsPrincipal();
            }

            return new ClaimsPrincipal(new ClaimsIdentity(_claimsGatherer.Gather(authenticationTicket.User)));
        }
    }
}