using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthCookieService<TAuthenticationTicket> : IAuthCookieService
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;

        public AuthCookieService(
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService
            )
        {
            _authenticationTicketService = authenticationTicketService;
        }

        public string CreateAuthCookie(TAuthenticationTicket authenticationTicket)
        {
            return $"{authenticationTicket.PersistenceToken};HttpOnly;Path=/";
        }

        /// <inheritdoc />
        public string CreateSignOutCookie()
        {
            return "Max-Age=0;Path=/";
        }

        /// <inheritdoc />
        public string CookieName => "scauthtoken";

        /// <inheritdoc />
        public bool TryReattachToTicketWithToken(IEnumerable<string> availableCookies)
        {
            if (_authenticationTicketService.GetCurrentAuthenticationTicket() != null)
            {
                return true;
            }
            var cookie = availableCookies
                .FirstOrDefault(c => c.StartsWith($"{CookieName}="));
            if (cookie == null)
            {
                return false;
            }
            var token = cookie
                .Split(new []{ '=' },2)[1] // '=' is guaranteed to exist because of the filter above
                .Split(';')[0]; // first part is always guaranteed to exist
            return _authenticationTicketService.AttachToToken(token);
        }

        public void ReattachOrCreate(IEnumerable<string> cookies)
        {
            if (!TryReattachToTicketWithToken(cookies))
            {
                var authenticationTicket = _authenticationTicketService.Create();
                Handle.AddOutgoingCookie(CookieName, CreateAuthCookie(authenticationTicket));
            }
        }
    }
}