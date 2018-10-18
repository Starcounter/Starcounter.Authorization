using System;

namespace Starcounter.Authorization.SignIn
{
    internal class SignInOptions
    {
        public TimeSpan NewTicketExpiration { get; set; } = TimeSpan.FromMinutes(15);
    }
}