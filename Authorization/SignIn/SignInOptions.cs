using System;

namespace Starcounter.Authorization.SignIn
{
    internal class SignInOptions
    {
        /// <summary>
        /// Authentication tickets will expire after this time of inactivity.
        /// </summary>
        public TimeSpan NewTicketExpiration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Interval of expired tickets cleanup. Null if no periodic cleanup should be performed.
        /// </summary>
        public TimeSpan? TicketCleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}