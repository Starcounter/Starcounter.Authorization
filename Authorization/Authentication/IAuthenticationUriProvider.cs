namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationUriProvider
    {
        string UnauthenticatedUriTemplate { get; }
        string UnauthenticatedViewUri { get; }
        string RedirectionViewUri { get; }
        Json CreateUnauthenticatedRedirection(string deniedUri);
    }
}