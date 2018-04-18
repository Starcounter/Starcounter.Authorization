using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public class SignInManager<TUserSession, TUser> : ISignInManager<TUserSession, TUser> where TUserSession : IUserSession<TUser> where TUser : IUserWithGroups
    {
        private readonly IUserClaimsGatherer _userClaimsGatherer;
        private readonly IStringSerializer<ClaimsPrincipal> _principalSerializer;
        private readonly ISystemClock _clock;
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ILogger<SignInManager<TUserSession, TUser>> _logger;
        private readonly SignInOptions _options;

        public SignInManager(IUserClaimsGatherer userClaimsGatherer,
            IStringSerializer<ClaimsPrincipal> principalSerializer,
            ISystemClock clock,
            IOptions<SignInOptions> options,
            ICurrentSessionProvider currentSessionProvider,
            ILogger<SignInManager<TUserSession, TUser>> logger)
        {
            _userClaimsGatherer = userClaimsGatherer;
            _principalSerializer = principalSerializer;
            _clock = clock;
            _currentSessionProvider = currentSessionProvider;
            _logger = logger;
            _options = options.Value;
        }
        public void SignIn(TUser user, TUserSession session)
        {
            try
            {
                var currentSessionSessionId = _currentSessionProvider.CurrentSessionId 
                                              ?? throw new InvalidOperationException("Current session is null");
                var principal = GeneratePrincipal(user);

                session.PrincipalSerialized = _principalSerializer.Serialize(principal);
                session.SessionId = currentSessionSessionId;
                session.ExpiresAt = (_clock.UtcNow + _options.NewTicketExpiration).UtcDateTime;
                session.User = user;
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