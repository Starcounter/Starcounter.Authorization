using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public class CurrentUserProvider<TUserSession, TUser> : ICurrentUserProvider<TUser> where TUserSession : class, IUserSession<TUser> where TUser : IUser
    {
        private readonly ICurrentSessionRetriever<TUserSession> _currentSessionRetriever;

        public CurrentUserProvider(ICurrentSessionRetriever<TUserSession> currentSessionRetriever)
        {
            _currentSessionRetriever = currentSessionRetriever;
        }
        public TUser GetCurrentUser()
        {
            return _currentSessionRetriever.GetCurrentSession().User;
        }
    }
}