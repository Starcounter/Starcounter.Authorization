using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationTicketProvider<out TAuthenticationTicket> 
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        /// <summary>
        /// Returns the authentication ticket for current session, or null if there is none.
        /// If the ticket is expired, this method will also return null
        /// </summary>
        /// <returns></returns>
        TAuthenticationTicket GetCurrentAuthenticationTicket();
    }
}