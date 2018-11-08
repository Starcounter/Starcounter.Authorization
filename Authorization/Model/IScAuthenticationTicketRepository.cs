using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    internal interface IScAuthenticationTicketRepository<TAuthenticationTicket>
    {
        TAuthenticationTicket FindBySession(Session session);
        TAuthenticationTicket FindByPersistenceToken(string token);
        void Delete(TAuthenticationTicket ticket);
        TAuthenticationTicket Create();
        void DeleteExpired(DateTime now);
        void AssociateWithSession(TAuthenticationTicket ticket, Session session);
    }
}