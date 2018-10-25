using System.Collections.Generic;

namespace Starcounter.Authorization.Authentication
{
    internal interface IAuthCookieService
    {
        /// <summary>
        /// Generates a persistent token for the current authentication ticket and associates it with said token.
        /// </summary>
        /// <returns>The cookie that should be sent to the browser. It is the value of the cookie and its attributes, meaning its name should be appended</returns>
        string CreateAuthCookie();

        /// <summary>
        /// Generates value for Set-Cookie header that should be sent to the browser to expire any stored auth cookie.
        /// Does not remove or invalidate the authentication ticket itself
        /// </summary>
        /// <returns>The cookie that should be sent to the browser. It is the value of the cookie and its attributes, meaning its name should be appended</returns>
        string CreateSignOutCookie();

        /// <summary>
        /// Name of the cookie that should be used with <see cref="CreateAuthCookie"/> and <see cref="CreateSignOutCookie"/>
        /// </summary>
        string CookieName { get; }

        /// <summary>
        /// Reattaches current session to the authentication ticket, given its token. Returns true if succeeded.
        /// </summary>
        /// <remarks>
        /// This will disassociate any previously associated session from the authentication ticket.
        /// If the passed cookie content is invalid or expired, this method will do nothing and return false.
        /// </remarks>
        /// <param name="availableCookies"></param>
        /// <returns>true if the session has been reattached, false otherwise</returns>
        bool TryReattachToTicketWithToken(IEnumerable<string> availableCookies);
    }
}