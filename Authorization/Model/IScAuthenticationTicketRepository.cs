using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    internal interface IScAuthenticationTicketRepository<TAuthenticationTicket>
    {
        TAuthenticationTicket FindBySessionId(string sessionId);
        TAuthenticationTicket FindByPersistenceToken(string token);
        void Delete(TAuthenticationTicket ticket);
        TAuthenticationTicket Create();
        void DeleteExpired(DateTime now);
    }
}