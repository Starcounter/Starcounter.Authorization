﻿using System;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class ScUserAuthenticationTicket : IScUserAuthenticationTicket<User>
    {
        public string SessionId { get; set; }
        public string PrincipalSerialized { get; set; }
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; }
    }
}