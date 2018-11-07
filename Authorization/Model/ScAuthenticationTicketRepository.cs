using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Starcounter.Linq;

namespace Starcounter.Authorization.Model
{
    internal class ScAuthenticationTicketRepository<TAuthenticationTicket> : IScAuthenticationTicketRepository<TAuthenticationTicket> 
        where TAuthenticationTicket : IScAuthenticationTicket, new()
    {
        public TAuthenticationTicket FindBySessionId(string sessionId)
        {
            return DbLinq.Objects<TAuthenticationTicket>()
                .FirstOrDefault(ticket => ticket.SessionId.Contains(sessionId));
        }

        public TAuthenticationTicket FindByPersistenceToken(string token)
        {
            return DbLinq.Objects<TAuthenticationTicket>()
                .FirstOrDefault(ticket => ticket.PersistenceToken == token);
        }

        public void Delete(TAuthenticationTicket ticket)
        {
            ticket.Delete();
        }

        public TAuthenticationTicket Create()
        {
            return new TAuthenticationTicket();
        }

        public void DeleteExpired(DateTime now)
        {
            DbLinq.Objects<TAuthenticationTicket>()
                .Delete(ticket => ticket.ExpiresAt < now);
        }
    }
}