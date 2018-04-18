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

    public interface ICurrentUserProvider<TUser> where TUser : IUser
    {
        TUser GetCurrentUser();
    }

    public class CurrentUserProvider<TUserSession, TUser> : ICurrentUserProvider<TUser> where TUserSession : class, IUserSession<TUser> where TUser : IUser
    {
        private readonly ICurrentSessionRetriever<TUserSession> _currentSessionRetriever;

        public CurrentUserProvider(ICurrentSessionRetriever<TUserSession> currentSessionRetriever)
        {
            _currentSessionRetriever = currentSessionRetriever;
        }
        public TUser GetCurrentUser()
        {
            return _currentSessionRetriever.GetCurrentSession().User;
        }
    }

    public interface ICurrentSessionRetriever<out TSession> where TSession : class, ISession
    {
        TSession GetCurrentSession();
    }

    public class CurrentSessionRetriever<TSession> : ICurrentSessionRetriever<TSession> where TSession : class, ISession
    {
        private readonly ICurrentSessionProvider _currentSessionProvider;
        private readonly ISystemClock _systemClock;
        private readonly ISessionRepository<TSession> _sessionRepository;
        private readonly ILogger _logger;

        public CurrentSessionRetriever(ICurrentSessionProvider currentSessionProvider,
            ISystemClock systemClock,
            ISessionRepository<TSession> sessionRepository,
            ILogger<CurrentSessionRetriever<TSession>> logger)
        {
            _currentSessionProvider = currentSessionProvider;
            _systemClock = systemClock;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public TSession GetCurrentSession()
        {
            var session = _sessionRepository.FindBySessionId(_currentSessionProvider.CurrentSessionId);
            if (session == null)
            {
                return null;
            }
            if (session.ExpiresAt < _systemClock.UtcNow)
            {
                _logger.LogInformation($"Found expired session. Removing");
                _sessionRepository.Delete(session);
                return null;
            }

            return session;
        }
    }
}