namespace Starcounter.Authorization
{
    public class AuthorizationSettings
    {
        /// <summary>
        /// Default value for <see cref="IAuthorizationSettings.NewTicketExpirationSeconds"/>
        /// </summary>
        public const long DefaultNewTicketExpirationSeconds = 15 * 60;

        /// <summary>
        /// Default value for <see cref="IAuthorizationSettings.TicketCleanupIntervalSeconds"/>
        /// </summary>
        public const long DefaultTicketCleanupIntervalSeconds = 1 * 60;

        /// <summary>
        /// Default value for anonymous ticket expiration, in seconds. Equals fifteen minutes.
        /// </summary>
        public const long DefaultAnonymousTicketExpiration = 15 * 60;
    }
}