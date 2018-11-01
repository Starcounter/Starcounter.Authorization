using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Implement this interface with a class specific to your application to enable retrieving signed in user
    /// </summary>
    public interface IScUserAuthenticationTicket<TUser> : IScAuthenticationTicket where TUser : IMinimalUser
    {
        TUser User { get; set; }
    }
}