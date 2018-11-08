namespace Starcounter.Authorization
{
    public interface ITicketToSession<TAuthenticationTicket>
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        string SessionId { get; set; }
        TAuthenticationTicket Ticket { get; set; }
    }
}