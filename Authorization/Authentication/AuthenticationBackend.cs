using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.Authentication
{
    public class AuthenticationBackend<TSession> : IAuthenticationBackend where TSession : class, ISession
    {
        private readonly IStringSerializer<ClaimsPrincipal> _principalSerializer;
        private readonly ICurrentSessionRetriever<TSession> _currentSessionRetriever;
        private readonly ILogger _logger;

        public AuthenticationBackend(ICurrentSessionRetriever<TSession> currentSessionRetriever,
            ILogger<AuthenticationBackend<TSession>> logger,
            IStringSerializer<ClaimsPrincipal> principalSerializer)
        {
            _currentSessionRetriever = currentSessionRetriever;
            _logger = logger;
            _principalSerializer = principalSerializer;
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var currentSession = _currentSessionRetriever.GetCurrentSession();
            if (currentSession == null)
            {
                return new ClaimsPrincipal();
            }
            return _principalSerializer.Deserialize(currentSession.PrincipalSerialized);
        }
    }
}