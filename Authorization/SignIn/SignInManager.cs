using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    internal class SignInManager<TAuthenticationTicket, TUser> : ISignInManager<TUser>
        where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>
        where TUser : IMinimalUser
    {
        private readonly ISystemClock _clock;
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ILogger _logger;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _authenticationTicketRepository;
        private readonly SignInOptions _options;

        public SignInManager(ISystemClock clock,
            IOptions<SignInOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ILogger<SignInManager<TAuthenticationTicket, TUser>> logger,
            IScAuthenticationTicketRepository<TAuthenticationTicket> authenticationTicketRepository)
        {
            _clock = clock;
            _currentSessionProvider = currentSessionProvider;
            _logger = logger;
            _authenticationTicketRepository = authenticationTicketRepository;
            _options = options.Value;
        }

        public void SignIn(TUser user)
        {
            try
            {
                var currentSessionSessionId = _currentSessionProvider.CurrentSessionId 
                                              ?? throw new InvalidOperationException("Current session is null");
                var authenticationTicket = _authenticationTicketRepository.Create();
                authenticationTicket.SessionId = currentSessionSessionId;
                authenticationTicket.ExpiresAt = (_clock.UtcNow + _options.NewTicketExpiration).UtcDateTime;
                authenticationTicket.User = user;
                _logger.LogInformation("User {User} signed in", user);
            }
            catch (Exception e)
            {
                _logger.LogError("Could not sign in user {User}", e, user);
                throw;
            }
        }
    }
}