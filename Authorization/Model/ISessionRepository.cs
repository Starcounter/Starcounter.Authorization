namespace Starcounter.Authorization.Model
{
    public interface ISessionRepository<TSession>
    {
        TSession FindBySessionId(string sessionId);
        void Delete(TSession session);
        TSession Create();
    }
}