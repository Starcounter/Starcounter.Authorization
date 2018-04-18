using System;

namespace Starcounter.Authorization.Model
{
    /// <summary>
    /// Implement this interface with a database Session class specific to your application.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Corresponds to <see cref="Session.SessionId"/>
        /// </summary>
        string SessionId { get; set; }
        
        /// <summary>
        /// base64-encoded and serialized <see cref="System.Security.Claims.ClaimsPrincipal"/>
        /// </summary>
        string PrincipalSerialized { get; set; }

        /// <summary>
        /// Expiry date of this session, in UTC
        /// </summary>
        DateTime ExpiresAt { get; set; }
    }
}