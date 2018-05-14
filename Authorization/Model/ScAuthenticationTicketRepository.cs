using System.Linq;

namespace Starcounter.Authorization.Model
{
    public class ScAuthenticationTicketRepository<TAuthenticationTicket> : IScAuthenticationTicketRepository<TAuthenticationTicket> 
        where TAuthenticationTicket : IScAuthenticationTicket, new()
    {
        public TAuthenticationTicket FindBySessionId(string sessionId)
        {
            // Starcounter.Linq fails due to https://github.com/Starcounter/Starcounter.Linq/issues/45 
            return Db.SQL<TAuthenticationTicket>($"select a from {typeof(TAuthenticationTicket).FullName.EscapeSql()} a where {nameof(IScAuthenticationTicket.SessionId).EscapeSql()} = ?", sessionId)
                .FirstOrDefault();
        }

        public TAuthenticationTicket FindByPersistenceToken(string token)
        {
            // Starcounter.Linq fails due to https://github.com/Starcounter/Starcounter.Linq/issues/45 
            return Db.SQL<TAuthenticationTicket>($"select a from {typeof(TAuthenticationTicket).FullName.EscapeSql()} a where {nameof(IScAuthenticationTicket.PersistenceToken).EscapeSql()} = ?", token)
                .FirstOrDefault();
        }

        public void Delete(TAuthenticationTicket ticket)
        {
            ticket.Delete();
        }

        public TAuthenticationTicket Create()
        {
            return new TAuthenticationTicket();
        }
    }
}