using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationTicketProvider<out TAuthenticationTicket> 
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        TAuthenticationTicket GetCurrentAuthenticationTicket();
    }
}