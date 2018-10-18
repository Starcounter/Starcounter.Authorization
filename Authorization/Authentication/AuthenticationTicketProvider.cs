using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.SignIn;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthenticationTicketProvider<TAuthenticationTicket> :
        IAuthenticationTicketProvider<TAuthenticationTicket>
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISystemClock _systemClock;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _scAuthenticationTicketRepository;
        private readonly ILogger _logger;
        private readonly ITransactionFactory _transactionFactory;
        private readonly SignInOptions _options;

        public AuthenticationTicketProvider(
            IOptions<SignInOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            IScAuthenticationTicketRepository<TAuthenticationTicket> scAuthenticationTicketRepository,
            ILogger<AuthenticationTicketProvider<TAuthenticationTicket>> logger,
            ITransactionFactory transactionFactory)
        {
            _options = options.Value;
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
            _transactionFactory.ExecuteTransaction(() => authenticationTicket.ExpiresAt = (_systemClock.UtcNow + _options.NewTicketExpiration).UtcDateTime);
            return authenticationTicket;
        }

        public TAuthenticationTicket EnsureTicket()
        {
            var existingTicket = GetCurrentAuthenticationTicket();
            if (existingTicket != null)
            {
                return existingTicket;
            }

            var currentSessionSessionId = _currentSessionProvider.CurrentSessionId
                                          ?? throw new InvalidOperationException("Current session is null");
            return _transactionFactory.ExecuteTransaction(() =>
            {
                var authenticationTicket = _scAuthenticationTicketRepository.Create();
                authenticationTicket.SessionId = currentSessionSessionId;
                authenticationTicket.ExpiresAt = (_systemClock.UtcNow + _options.NewTicketExpiration).UtcDateTime;
                return authenticationTicket;
            });
        }
    }
}