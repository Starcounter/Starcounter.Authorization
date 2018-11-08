namespace Starcounter.Authorization
{
    internal class DefaultCurrentSessionProvider : ICurrentSessionProvider
    {
        public Session CurrentSession => Session.Ensure();
    }
}
