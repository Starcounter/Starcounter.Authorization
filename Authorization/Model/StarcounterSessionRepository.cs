using System;
using System.Linq;
using System.Linq.Expressions;
using Starcounter.Linq;

namespace Starcounter.Authorization.Model
{
    public class StarcounterSessionRepository<TSession> : ISessionRepository<TSession> where TSession : ISession, new()
    {
        public TSession FindBySessionId(string sessionId)
        {
            var parameterExpression = Expression.Parameter(typeof(TSession));
            var expression = Expression.Lambda<Func<TSession, bool>>(Expression.ReferenceEqual(Expression.Property(
                        parameterExpression,
                        typeof(ISession).GetProperty(nameof(ISession.SessionId)) ?? throw new InvalidOperationException("SessionId property is null")),
                    Expression.Constant(sessionId)),
                parameterExpression);
            return DbLinq.Objects<TSession>()
                .FirstOrDefault(expression);
//            return DbLinq.Objects<TSession>()
//                .Where(session => session.SessionId == sessionId)
//                .FirstOrDefault();
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