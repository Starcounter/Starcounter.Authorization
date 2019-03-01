using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Settings;
using Starcounter.Authorization.SignIn;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthenticationTicketService<TAuthenticationTicket, TUser> :
        IAuthenticationTicketService<TAuthenticationTicket>
        where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>
        where TUser : IMinimalUser
    {
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISystemClock _systemClock;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _scAuthenticationTicketRepository;
        private readonly ILogger _logger;
        private readonly ISecureRandom _secureRandom;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IOptions<AuthorizationOptions> _options;

        public AuthenticationTicketService
        (
            IOptions<AuthorizationOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            IScAuthenticationTicketRepository<TAuthenticationTicket> scAuthenticationTicketRepository,
            ILogger<AuthenticationTicketService<TAuthenticationTicket, TUser>> logger,
            ISecureRandom secureRandom,
            ITransactionFactory transactionFactory
        )
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
            Session currentSession = _currentSessionProvider.CurrentSession;
            if (currentSession == null)
            {
                return default(TAuthenticationTicket);
            }

            TAuthenticationTicket authenticationTicket = _scAuthenticationTicketRepository.FindBySession(currentSession);
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

            _transactionFactory.ExecuteTransaction(() =>
            {
                TimeSpan addToExpiration = authenticationTicket.User == null ? _options.Value.AnonymousTicketExpiration : _options.Value.AuthenticatedTicketExpiration;
                DateTime result = authenticationTicket.ExpiresAt = (_systemClock.UtcNow + addToExpiration).UtcDateTime;

                return result;
            });

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
            Session currentSession = _currentSessionProvider.CurrentSession;

            return _transactionFactory.ExecuteTransaction(() =>
            {
                TAuthenticationTicket existingTicket = _scAuthenticationTicketRepository.FindByPersistenceToken(token);

                if (existingTicket == null)
                {
                    return false;
                }

                _scAuthenticationTicketRepository.AssociateWithSession(existingTicket, currentSession);

                return true;
            });
        }

        public TAuthenticationTicket Create()
        {
            Session currentSession = _currentSessionProvider.CurrentSession ?? throw new InvalidOperationException("Current session is null");

            return _transactionFactory.ExecuteTransaction(() =>
            {
                TAuthenticationTicket authenticationTicket = _scAuthenticationTicketRepository.Create();
                authenticationTicket.ExpiresAt = (_systemClock.UtcNow + _options.Value.AnonymousTicketExpiration).UtcDateTime;

                // Source: https://www.owasp.org/index.php/Session_Management_Cheat_Sheet#Session_ID_Length
                int bytesLength = 16;
                authenticationTicket.PersistenceToken = _secureRandom.GenerateRandomHexString(bytesLength);
                _scAuthenticationTicketRepository.AssociateWithSession(authenticationTicket, currentSession);

                return authenticationTicket;
            });
        }
    }
}