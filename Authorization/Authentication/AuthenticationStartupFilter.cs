using System;
using System.Net;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Authentication
{
    public class AuthenticationStartupFilter : IStartupFilter
    {
        private const string UnauthenticatedPageViewContent = @"<template><h1>You need to sign in to access this page. Please contact your administrator to find out how to do that</h1></template>";
        private const string RedirectionPageViewContent = @"<link rel=""import"" href=""/sys/palindrom-redirect/palindrom-redirect.html"">"
                                                          + @"<template><dom-bind><template is=""dom-bind"">"
                                                          + @"<palindrom-redirect history url$=""{{model.RedirectUrl}}""></palindrom-redirect>"
                                                          + @"</template></dom-bind></template>";
        private readonly IAuthenticationUriProvider _authenticationUriProvider;

        public AuthenticationStartupFilter(IAuthenticationUriProvider authenticationUriProvider)
        {
            _authenticationUriProvider = authenticationUriProvider;
        }
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                RegisterHtmlPage(_authenticationUriProvider.RedirectionViewUri, RedirectionPageViewContent);
                RegisterHtmlPage(_authenticationUriProvider.UnauthenticatedViewUri, UnauthenticatedPageViewContent);

                Blender.MapUri(_authenticationUriProvider.UnauthenticatedUriTemplate, string.Empty, new[] { "redirection" });
                Handle.GET(_authenticationUriProvider.UnauthenticatedUriTemplate,
                    (string redirectToUnused) => new Json
                    {
                        ["Html"] = _authenticationUriProvider.UnauthenticatedViewUri
                    });
                
                next(app);
            };
        }

        private void RegisterHtmlPage(string uri, string redirectionPageViewContent)
        {
            Handle.GET(uri,
                () => {
                    var response = Response.FromStatusCode((int) HttpStatusCode.OK, redirectionPageViewContent);
                    response.ContentType = "text/html";
                    return response;
                });
        }
    }
}