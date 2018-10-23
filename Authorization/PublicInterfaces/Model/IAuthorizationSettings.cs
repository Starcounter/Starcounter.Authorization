using System;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Implement this interface and register your implementation with <see cref="AuthenticationServiceCollectionExtensions.AddAuthorizationSettings{TAuthorizationSettings}"/>
    /// to use settings from the database. If you won't, default settings will be used. See <see cref="AuthorizationSettings"/> for default values.
    /// </summary>
    public interface IAuthorizationSettings
    {
        /// <summary>
        /// Authentication tickets will expire after this time of inactivity.
        /// </summary>
        long NewTicketExpirationSeconds { get; set; }

        /// <summary>
        /// Interval of expired tickets cleanup. Zero if no periodic cleanup should be performed.
        /// </summary>
        long TicketCleanupIntervalSeconds { get; set; }
    }
}