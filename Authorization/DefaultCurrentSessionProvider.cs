namespace Starcounter.Authorization
{
    public class DefaultCurrentSessionProvider : ICurrentSessionProvider
    {
        public string CurrentSessionId => Session.Current?.SessionId;
    }
}
