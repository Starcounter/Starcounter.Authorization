namespace Starcounter.Authorization
{
    internal class DefaultCurrentSessionProvider : ICurrentSessionProvider
    {
        public string CurrentSessionId => Session.Current?.SessionId;
    }
}
