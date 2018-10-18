using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationTicketProvider<out TAuthenticationTicket> 
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        /// <summary>
        /// Returns the authentication ticket for current session, or null if there is none.
        /// If the ticket is expired, this method will also return null
        /// </summary>
        /// <returns></returns>
        TAuthenticationTicket GetCurrentAuthenticationTicket();

        /// <summary>
        /// Returns the authentication ticket for current session, creating one if necessary.
        /// If the preexisting ticket is expired, this method will delete it and return a new one.
        /// </summary>
        /// <returns></returns>
        TAuthenticationTicket EnsureTicket();
    }
}