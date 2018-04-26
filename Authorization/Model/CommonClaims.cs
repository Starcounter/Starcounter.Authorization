namespace Starcounter.Authorization.Model
{
    /// <summary>
    /// Common claims types that may be used in applications
    /// </summary>
    public static class CommonClaims
    {
        /// <summary>
        /// Anyone bearing this claim should be regarded as system-wide administrator
        /// </summary>
        public const string SuperuserClaimType = "http://schema.starcounter.com/Starcounter.Authorization.Superuser";
    }
}