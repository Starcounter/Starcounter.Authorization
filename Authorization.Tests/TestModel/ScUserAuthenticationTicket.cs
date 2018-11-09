using System;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class ScUserAuthenticationTicket : IScUserAuthenticationTicket<User>
    {
        public DateTime ExpiresAt { get; set; }
        public string PersistenceToken { get; set; }
        public User User { get; set; }
    }
}