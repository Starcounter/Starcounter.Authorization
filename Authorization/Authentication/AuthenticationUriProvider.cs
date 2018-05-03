using System.Web;

namespace Starcounter.Authorization.Authentication
{
    public class AuthenticationUriProvider : IAuthenticationUriProvider
    {
        public string UnauthenticatedUriTemplate => $"/{Application.Current}/Starcounter.Authorization.Unauthenticated/{{?}}";

        public string UnauthenticatedViewUri =>
            $"/{Application.Current}/Starcounter.Authorization.Unauthenticated.html";

        public string RedirectionViewUri =>
            $"/{Application.Current}/Starcounter.Authorization.Redirection.html";

        public Json CreateUnauthenticatedRedirection(string deniedUri) => new Json
        {
            ["Html"] = RedirectionViewUri,
            ["RedirectUrl"] = UnauthenticatedUriTemplate.Replace("{?}", HttpUtility.UrlEncode(deniedUri))
        };
    }
}