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
    }
}