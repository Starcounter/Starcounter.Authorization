namespace Starcounter.Authorization.Model
{
    /// <summary>
    /// Implement this interface with a class specific to your application to enable retrieving signed in user
    /// </summary>
    public interface IScUserAuthenticationTicket<TUser>: IScAuthenticationTicket where TUser:IUser
    {
        TUser User { get; set; }
    }
}