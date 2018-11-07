using System;

namespace Starcounter.Authorization.Settings
{

    /// <summary>
    /// Holds the settings for authorization in runtime. This is different from <see cref="IAuthorizationSettings"/> to support
    /// case where the app doesn't register any db class implementing <see cref="IAuthorizationSettings"/>.
    /// </summary>
    internal class AuthorizationOptions
    {
        /// <summary>
        /// Authenticated tickets (i.e. those with non-null User) will expire after this time of inactivity.
        /// </summary>
        public TimeSpan AuthenticatedTicketExpiration { get; set; } =
            TimeSpan.FromSeconds(AuthorizationSettings.DefaultNewTicketExpirationSeconds);
        
        /// <summary>
        /// Anonymous tickets (i.e. those with null User) will expire after this time of inactivity.
        /// </summary>
        public TimeSpan AnonymousTicketExpiration { get; set; } =
            TimeSpan.FromSeconds(AuthorizationSettings.DefaultAnonymousTicketExpiration);
        
        /// <summary>
        /// Interval of expired tickets cleanup. 0 if no periodic cleanup should be performed.
        /// </summary>
        public TimeSpan TicketCleanupInterval { get; set; } =
            TimeSpan.FromSeconds(AuthorizationSettings.DefaultTicketCleanupIntervalSeconds);
    }
}