namespace Starcounter.Authorization.Tests.TestModel
{
    public class TestSettings:IAuthorizationSettings
    {
        public long NewTicketExpirationSeconds { get; set; }
        public long TicketCleanupIntervalSeconds { get; set; }
    }
}