using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Settings;
using Starcounter.Authorization.SignIn;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthenticationTicketService<TAuthenticationTicket> :
        IAuthenticationTicketService<TAuthenticationTicket>
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISystemClock _systemClock;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _scAuthenticationTicketRepository;
        private readonly ILogger _logger;
        private readonly ISecureRandom _secureRandom;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IOptions<AuthorizationOptions> _options;

        public AuthenticationTicketService(
            IOptions<AuthorizationOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            IScAuthenticationTicketRepository<TAuthenticationTicket> scAuthenticationTicketRepository,
            ILogger<AuthenticationTicketService<TAuthenticationTicket>> logger,
            ISecureRandom secureRandom,
            ITransactionFactory transactionFactory)
        {
            _options = options;
            _currentSessionProvider = currentSessionProvider;
            _systemClock = systemClock;
            _scAuthenticationTicketRepository = scAuthenticationTicketRepository;
            _logger = logger;
            _secureRandom = secureRandom;
            _transactionFactory = transactionFactory;
        }

        /// <inheritdoc />
        public TAuthenticationTicket GetCurrentAuthenticationTicket()
        {
            var starcounterSessionId = _currentSessionProvider.CurrentSessionId;
            if (starcounterSessionId == null)
            {
                return default(TAuthenticationTicket);
            }
            var authenticationTicket = _scAuthenticationTicketRepository.FindBySessionId(starcounterSessionId);
            if (authenticationTicket == null)
            {
                return default(TAuthenticationTicket);
            }

            // Specifying Kind manually, because of https://github.com/Starcounter/level1/issues/4798
            // when this bug is fixed, we can simplify the line below
            if (DateTime.SpecifyKind(authenticationTicket.ExpiresAt, DateTimeKind.Utc) < _systemClock.UtcNow)
            {
                _logger.LogInformation($"Found expired authentication ticket. Removing");
                _transactionFactory.ExecuteTransaction(() => _scAuthenticationTicketRepository.Delete(authenticationTicket));
                return default(TAuthenticationTicket);
            }
            _transactionFactory.ExecuteTransaction(() => authenticationTicket.ExpiresAt = (_systemClock.UtcNow + _options.Value.NewTicketExpiration).UtcDateTime);
            return authenticationTicket;
        }

        /// <inheritdoc />
        public void CleanExpiredTickets()
        {
            _transactionFactory.ExecuteTransaction(() =>
                {
                    _scAuthenticationTicketRepository.DeleteExpired(_systemClock.UtcNow.UtcDateTime);
                });
        }

        public bool AttachToToken(string token)
        {
            var currentSessionId = _currentSessionProvider.CurrentSessionId;
            return _transactionFactory.ExecuteTransaction(() =>
            {
                var existingTicket = _scAuthenticationTicketRepository.FindByPersistenceToken(token);
                if (existingTicket == null)
                {
                    return false;
                }
                if (!existingTicket.SessionId.Contains(currentSessionId))
                {
                    existingTicket.SessionId += ";" + currentSessionId;
                }

                return true;
            });
        }

        public TAuthenticationTicket Create()
        {
            var currentSessionId = _currentSessionProvider.CurrentSessionId ?? throw new InvalidOperationException("Current session is null");
            return _transactionFactory.ExecuteTransaction(() =>
            {
                var authenticationTicket = _scAuthenticationTicketRepository.Create();
                authenticationTicket.SessionId = currentSessionId;
                authenticationTicket.ExpiresAt = (_systemClock.UtcNow + _options.Value.NewTicketExpiration).UtcDateTime;
                // Source: https://www.owasp.org/index.php/Session_Management_Cheat_Sheet#Session_ID_Length
                var bytesLength = 16;
                authenticationTicket.PersistenceToken = _secureRandom.GenerateRandomHexString(bytesLength);

                return authenticationTicket;
            });
        }
    }
}