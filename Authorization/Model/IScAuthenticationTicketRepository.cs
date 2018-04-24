namespace Starcounter.Authorization.Model
{
    public interface IScAuthenticationTicketRepository<TAuthenticationTicket>
    {
        TAuthenticationTicket FindBySessionId(string sessionId);
        void Delete(TAuthenticationTicket ticket);
        TAuthenticationTicket Create();
    }
}