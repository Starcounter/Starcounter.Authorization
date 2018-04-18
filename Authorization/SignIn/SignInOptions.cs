using System;

namespace Starcounter.Authorization.SignIn
{
    public class SignInOptions
    {
        public string AuthenticationType { get; set; } = "Starcounter";
        public TimeSpan NewTicketExpiration { get; set; } = TimeSpan.FromDays(7);
    }
}