namespace Starcounter.Authorization.Core
{
    public static class AuthorizationStatic
    {
        public static AuthorizationEnforcement Enforcement { get; set; }
        public static IAuthorizationConfigurator Rules { get; set; }
    }
}