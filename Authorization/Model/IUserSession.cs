namespace Starcounter.Authorization.Model
{
    public interface IUserSession<TUser>: ISession where TUser:IUser
    {
        TUser User { get; set; }
    }
}