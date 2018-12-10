using Starcounter;
using Starcounter.Authorization;

namespace Application
{
    [Database]
    public class AuthorizationSettings: IAuthorizationSettings
    {
        public long NewTicketExpirationSeconds { get; set; }
        public long TicketCleanupIntervalSeconds { get; set; }
    }
}