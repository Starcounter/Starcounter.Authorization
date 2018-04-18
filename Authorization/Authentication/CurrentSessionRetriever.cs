using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
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
            var starcounterSessionId = _currentSessionProvider.CurrentSessionId;
            if (starcounterSessionId == null)
            {
                return null;
            }
            var session = _sessionRepository.FindBySessionId(starcounterSessionId);
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