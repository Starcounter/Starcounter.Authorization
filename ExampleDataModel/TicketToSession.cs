using Starcounter;
using Starcounter.Authorization;

namespace Application
{
    [Database]
    public class TicketToSession: ITicketToSession<UserSession>
    {
        public string SessionId { get; set; }
        public UserSession Ticket { get; set; }
    }
}