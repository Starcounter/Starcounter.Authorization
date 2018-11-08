namespace Starcounter.Authorization.Tests.TestModel
{
    public class TicketToSession: ITicketToSession<ScUserAuthenticationTicket>
    {
        public string SessionId { get; set; }
        public ScUserAuthenticationTicket Ticket { get; set; }
    }
}