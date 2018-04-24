using System;
using System.Security.Claims;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.Authentication
{
    public class AuthenticationBackend<TAuthenticationTicket> : IAuthenticationBackend
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IStringSerializer<ClaimsPrincipal> _principalSerializer;
        private readonly IAuthenticationTicketProvider<TAuthenticationTicket> _authenticationTicketProvider;

        public AuthenticationBackend(IAuthenticationTicketProvider<TAuthenticationTicket> authenticationTicketProvider,
            IStringSerializer<ClaimsPrincipal> principalSerializer)
        {
            _authenticationTicketProvider = authenticationTicketProvider;
            _principalSerializer = principalSerializer;
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var authenticationTicket = _authenticationTicketProvider.GetCurrentAuthenticationTicket();
            if (authenticationTicket == null)
            {
                return new ClaimsPrincipal();
            }
            return _principalSerializer.Deserialize(authenticationTicket.PrincipalSerialized);
        }
    }
}