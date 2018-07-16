using System.Security.Claims;

namespace Starcounter.Authorization.Authentication
{
    /// <summary>
    /// Possible values for <see cref="ClaimsIdentity.AuthenticationType"/>.
    /// </summary>
    public class AuthenticationTypes
    {
        /// <summary>
        /// Default value for <see cref="ClaimsIdentity.AuthenticationType"/> used by this library.
        /// </summary>
        public const string Starcounter = "Starcounter";
    }
}