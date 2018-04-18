using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public interface ICurrentSessionRetriever<out TSession> where TSession : class, ISession
    {
        TSession GetCurrentSession();
    }
}