namespace Starcounter.Authorization.Model
{
    /// <summary>
    /// Implement this interface with a Session class specific to your application to enable retrieving signed in user
    /// </summary>
    public interface IUserSession<TUser>: ISession where TUser:IUser
    {
        TUser User { get; set; }
    }
}