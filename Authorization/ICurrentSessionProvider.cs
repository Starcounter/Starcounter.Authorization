namespace Starcounter.Authorization
{
    public interface ICurrentSessionProvider
    {
        string CurrentSessionId { get; }
    }
}