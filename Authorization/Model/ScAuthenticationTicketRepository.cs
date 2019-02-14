using System;
using System.Linq;
using Starcounter.Linq;

namespace Starcounter.Authorization.Model
{
    internal class ScAuthenticationTicketRepository<TAuthenticationTicket, TTicketToSession> : IScAuthenticationTicketRepository<TAuthenticationTicket> 
        where TAuthenticationTicket : class, IScAuthenticationTicket, new()
        where TTicketToSession : ITicketToSession<TAuthenticationTicket>, new()
    {
        public TAuthenticationTicket FindBySession(Session session)
        {
            return Db.SQL<TAuthenticationTicket>(
                $"select a.{nameof(ITicketToSession<TAuthenticationTicket>.Ticket)} from {typeof(TTicketToSession)}" +
                $" a where {nameof(ITicketToSession<TAuthenticationTicket>.SessionId)} = ?",
                session.SessionId)
                .FirstOrDefault();
        }

        public TAuthenticationTicket FindByPersistenceToken(string token)
        {
            return DbLinq.Objects<TAuthenticationTicket>()
                .FirstOrDefault(ticket => ticket.PersistenceToken == token);
        }

        public void Delete(TAuthenticationTicket ticket)
        {
            foreach (var ticketToSession in DbLinq.Objects<TTicketToSession>().Where(tts => tts.Ticket == ticket))
            {
                ticketToSession.Delete();
            }

            ticket.Delete();
        }

        public TAuthenticationTicket Create()
        {
            return new TAuthenticationTicket();
        }

        public void DeleteExpired(DateTime now)
        {
            foreach (var ticket in
                DbLinq.Objects<TAuthenticationTicket>()
                    .Where(o => o.ExpiresAt < now))
            {
                Delete(ticket);
            }
        }

        public void AssociateWithSession(TAuthenticationTicket ticket, Session session)
        {
            new TTicketToSession()
            {
                SessionId = session.SessionId,
                Ticket = ticket
            };
        }
    }
}