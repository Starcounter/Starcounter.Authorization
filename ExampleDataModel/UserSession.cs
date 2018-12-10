using System;
using Starcounter;
using Starcounter.Authorization;

namespace Application
{
    [Database]
    public class UserSession: IScUserAuthenticationTicket<User>
    {
        public DateTime ExpiresAt { get; set; }
        public string PersistenceToken { get; set; }
        public User User { get; set; }
    }
}
