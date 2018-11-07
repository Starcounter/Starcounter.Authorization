using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationTicketService<out TAuthenticationTicket> 
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        /// <summary>
        /// Returns the authentication ticket for current session, or null if there is none.
        /// If the ticket is expired, this method will also return null
        /// </summary>
        /// <returns></returns>
        TAuthenticationTicket GetCurrentAuthenticationTicket();

        /// <summary>
        /// Removes all expired tickets. This method is always safe to execute, but must be invoked on
        /// a scheduler thread.
        /// </summary>
        void CleanExpiredTickets();

        /// <summary>
        /// Finds a ticket using supplied token and attaches it to the current session. Returns true if ticket
        /// was found.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>true if ticket with supplied token was found and attached</returns>
        bool AttachToToken(string token);

        /// <summary>
        /// Creates a brand new ticket.
        /// </summary>
        /// <returns></returns>
        TAuthenticationTicket Create();
    }
}