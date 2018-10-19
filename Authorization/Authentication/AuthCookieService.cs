using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;
using Starcounter.XSON.Interfaces;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthCookieService<TAuthenticationTicket> : IAuthCookieService
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _authenticationTicketRepository;
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISecureRandom _secureRandom;
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly ITransactionFactory _transactionFactory;
        public const string CookieName = "scauthtoken";

        public AuthCookieService(
            IScAuthenticationTicketRepository<TAuthenticationTicket> authenticationTicketRepository,
            ICurrentSessionProvider currentSessionProvider,
            ISecureRandom secureRandom,
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            ITransactionFactory transactionFactory)
        {
            _authenticationTicketRepository = authenticationTicketRepository;
            _currentSessionProvider = currentSessionProvider;
            _secureRandom = secureRandom;
            _authenticationTicketService = authenticationTicketService;
            _transactionFactory = transactionFactory;
        }

        public string CreateAuthCookie()
        {
            var ticket = _authenticationTicketService.GetCurrentAuthenticationTicket();
            if (ticket == null)
            {
                return null;
            }
            // Source: https://www.owasp.org/index.php/Session_Management_Cheat_Sheet#Session_ID_Length
            var bytesLength = 16;
            var token = _secureRandom.GenerateRandomHexString(bytesLength);
            _transactionFactory.ExecuteTransaction(() => ticket.PersistenceToken = token);
            return $"{CookieName}={token};HttpOnly;Path=/";
        }

        public string CreateSignOutCookie()
        {
            return $"{CookieName}=;Max-Age=0;Path=/";
        }

        public bool TryReattachToTicketWithToken(IEnumerable<string> availableCookies)
        {
            var cookie = availableCookies
                .FirstOrDefault(c => c.StartsWith($"{CookieName}="));
            if (cookie == null)
            {
                return false;
            }
            var token = cookie
                .Split(new []{ '=' },2)[1] // '=' is guaranteed to exist because of the filter above
                .Split(';')[0]; // first part is always guaranteed to exist
            var ticket = _authenticationTicketRepository.FindByPersistenceToken(token);
            if (ticket == null)
            {
                return false;
            }

            _transactionFactory.ExecuteTransaction(() => ticket.SessionId = _currentSessionProvider.CurrentSessionId);
            return true;
        }
    }
}