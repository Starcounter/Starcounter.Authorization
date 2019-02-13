using System;
using System.Net;
using System.Web;
using Microsoft.Extensions.Options;
using Starcounter.Startup.Routing.Middleware;

namespace Starcounter.Authorization.Middleware
{
    internal class DefaultSecurityMiddlewareOptions: IPostConfigureOptions<SecurityMiddlewareOptions>
    {
        private readonly IAuthenticationUriProvider _authenticationUriProvider;

        public DefaultSecurityMiddlewareOptions(IAuthenticationUriProvider authenticationUriProvider)
        {
            _authenticationUriProvider = authenticationUriProvider;
        }

        public void PostConfigure(string name, SecurityMiddlewareOptions options)
        {
            if (options.UnauthenticatedResponseCreator == null)
            {
                options.UnauthenticatedResponseCreator = CreateDefaultUnauthenticatedResponse;
            }
            if (options.UnauthorizedResponseCreator == null)
            {
                options.UnauthorizedResponseCreator = CreateDefaultUnauthorizedResponse;
            }
        }

        private Json CreateDefaultUnauthenticatedResponse(string deniedUri)
        {
            return new Json
            {
                ["Html"] = _authenticationUriProvider.RedirectionViewUri,
                ["RedirectUrl"] = UriHelper.WithArguments(_authenticationUriProvider.UnauthenticatedUriTemplate, HttpUtility.UrlEncode(deniedUri))
            };
        }

        private Response CreateDefaultUnauthorizedResponse()
        {
            var response = Response.FromStatusCode((int)HttpStatusCode.NotFound, Fetch404Template());
            response.ContentType = "text/html";
            return response;
        }

        private static string Fetch404Template()
        {
            try
            {
                string appShellHTMLUrl = "/sys/error/404.html";
                return Self.GET(appShellHTMLUrl).Body;
            }
            catch
            {
                return "Page not found";
            }
        }
    }
}