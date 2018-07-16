namespace Starcounter.Authorization
{
    internal interface ICurrentSessionProvider
    {
        string CurrentSessionId { get; }
    }
}