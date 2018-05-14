namespace Starcounter.Authorization.Model
{
    public interface IScAuthenticationTicketRepository<TAuthenticationTicket>
    {
        TAuthenticationTicket FindBySessionId(string sessionId);
        TAuthenticationTicket FindByPersistenceToken(string token);
        void Delete(TAuthenticationTicket ticket);
        TAuthenticationTicket Create();
    }
}