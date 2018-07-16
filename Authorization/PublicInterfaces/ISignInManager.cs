using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Use this service in your custom SignIn application to mark user as authenticated.
    /// This method adds to current session an authentication ticket associated with 
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public interface ISignInManager<in TUser>
        where TUser : IMinimalUser
    {
        /// <summary>
        /// This method adds to current session an authentication ticket associated with <paramref name="user"/>.
        /// Call this method after verifying user's identity
        /// </summary>
        /// <param name="user">The user to be authenticated</param>
        void SignIn(TUser user);
    }
}