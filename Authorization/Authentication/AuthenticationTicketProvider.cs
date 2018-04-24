using Microsoft.Extensions.Logging;
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

        public AuthenticationTicketProvider(ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            IScAuthenticationTicketRepository<TAuthenticationTicket> scAuthenticationTicketRepository,
            ILogger<AuthenticationTicketProvider<TAuthenticationTicket>> logger)
        {
            _currentSessionProvider = currentSessionProvider;
            _systemClock = systemClock;
            _scAuthenticationTicketRepository = scAuthenticationTicketRepository;
            _logger = logger;
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
                _scAuthenticationTicketRepository.Delete(authenticationTicket);
                return null;
            }

            return authenticationTicket;
        }
    }
}