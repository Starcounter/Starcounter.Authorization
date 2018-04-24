using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public class SignInManager<TAuthenticationTicket, TUser> : ISignInManager<TAuthenticationTicket, TUser>
        where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>
        where TUser : IUserWithGroups
    {
        private readonly IUserClaimsGatherer _userClaimsGatherer;
        private readonly IClaimsPrincipalSerializer _principalSerializer;
        private readonly ISystemClock _clock;
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ILogger _logger;
        private readonly SignInOptions _options;

        public SignInManager(IUserClaimsGatherer userClaimsGatherer,
            IClaimsPrincipalSerializer principalSerializer,
            ISystemClock clock,
            IOptions<SignInOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ILogger<SignInManager<TAuthenticationTicket, TUser>> logger)
        {
            _userClaimsGatherer = userClaimsGatherer;
            _principalSerializer = principalSerializer;
            _clock = clock;
            _currentSessionProvider = currentSessionProvider;
            _logger = logger;
            _options = options.Value;
        }
        public void SignIn(TUser user, TAuthenticationTicket authenticationTicket)
        {
            try
            {
                var currentSessionSessionId = _currentSessionProvider.CurrentSessionId 
                                              ?? throw new InvalidOperationException("Current session is null");
                var principal = GeneratePrincipal(user);

                authenticationTicket.PrincipalSerialized = _principalSerializer.Serialize(principal);
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

        private ClaimsPrincipal GeneratePrincipal(TUser user)
        {
            var claimsIdentity = new ClaimsIdentity(_options.AuthenticationType);
            claimsIdentity.AddClaims(_userClaimsGatherer.Gather(user));
            var principal = new ClaimsPrincipal(claimsIdentity);
            return principal;
        }
    }
}