namespace Starcounter.Authorization
{
    internal interface ICurrentSessionProvider
    {
        Session CurrentSession { get; }
    }
}