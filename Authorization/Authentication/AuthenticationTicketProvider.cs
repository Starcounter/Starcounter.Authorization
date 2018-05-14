using Microsoft.Extensions.Logging;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public class AuthenticationTicketProvider<TAuthenticationTicket> :
        IAuthenticationTicketProvider<TAuthenticationTicket>
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISystemClock _systemClock;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _scAuthenticationTicketRepository;
        private readonly ILogger _logger;
        private readonly ITransactionFactory _transactionFactory;

        public AuthenticationTicketProvider(ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            IScAuthenticationTicketRepository<TAuthenticationTicket> scAuthenticationTicketRepository,
            ILogger<AuthenticationTicketProvider<TAuthenticationTicket>> logger,
            ITransactionFactory transactionFactory)
        {
            _currentSessionProvider = currentSessionProvider;
            _systemClock = systemClock;
            _scAuthenticationTicketRepository = scAuthenticationTicketRepository;
            _logger = logger;
            _transactionFactory = transactionFactory;
        }

        public TAuthenticationTicket GetCurrentAuthenticationTicket()
        {
            var starcounterSessionId = _currentSessionProvider.CurrentSessionId;
            if (starcounterSessionId == null)
            {
                return null;
            }
            var authenticationTicket = _scAuthenticationTicketRepository.FindBySessionId(starcounterSessionId);
            if (authenticationTicket == null)
            {
                return null;
            }
            if (authenticationTicket.ExpiresAt < _systemClock.UtcNow)
            {
                _logger.LogInformation($"Found expired authentication ticket. Removing");
                _transactionFactory.ExecuteTransaction(() => _scAuthenticationTicketRepository.Delete(authenticationTicket));
                return null;
            }

            return authenticationTicket;
        }
    }
}