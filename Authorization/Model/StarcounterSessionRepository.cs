using System.Linq;

namespace Starcounter.Authorization.Model
{
    public class StarcounterSessionRepository<TSession> : ISessionRepository<TSession> where TSession : ISession, new()
    {
        public TSession FindBySessionId(string sessionId)
        {
            // Starcounter.Linq fails due to https://github.com/Starcounter/Starcounter.Linq/issues/45 
            return Db.SQL<TSession>($"select a from {typeof(TSession).FullName.EscapeSql()} a where {nameof(ISession.SessionId).EscapeSql()} = ?", sessionId)
                .FirstOrDefault();
        }

        public void Delete(TSession session)
        {
            session.Delete();
        }

        public TSession Create()
        {
            return new TSession();
        }
    }
}