using System;
using System.Net;
using System.Web;
using Microsoft.Extensions.Logging;
using Starcounter.Advanced;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthenticationStartupFilter : IStartupFilter
    {
        private const string UnauthenticatedPageViewContent = @"";
        private const string RedirectionPageViewContent = @"<link rel=""import"" href=""/sys/palindrom-redirect/palindrom-redirect.html"">"
                                                          + @"<template><dom-bind><template is=""dom-bind"">"
                                                          + @"<palindrom-redirect history url$=""{{model.RedirectUrl}}""></palindrom-redirect>"
                                                          + @"</template></dom-bind></template>";
        private readonly IAuthenticationUriProvider _authenticationUriProvider;
        private readonly IAuthCookieService _authCookieService;
        private readonly ISignOutService _signOutService;

        public AuthenticationStartupFilter(
            IAuthenticationUriProvider authenticationUriProvider,
            IAuthCookieService authCookieService,
            ISignOutService signOutService)
        {
            _authenticationUriProvider = authenticationUriProvider;
            _authCookieService = authCookieService;
            _signOutService = signOutService;
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
                // middleware will set the auth cookie
                Handle.GET(_authenticationUriProvider.SetTokenUriTemplate,
                    (string redirectTo, Request request) => CreateRedirectionResponse(redirectTo));
                Handle.GET(_authenticationUriProvider.SignOutUriTemplate,
                    (string redirectTo, Request request) => {
                        var response = CreateRedirectionResponse(redirectTo);
                        _signOutService.SignOut();
                        Handle.AddOutgoingCookie(_authCookieService.CookieName, _authCookieService.CreateSignOutCookie());
                        return response;
                    });
                Application.Current.Use(request =>
                {
                    if (request.IsStaticFileRequest)
                    {
                        return null;
                    }
                    Session.Ensure();
                    _authCookieService.ReattachOrCreate(request.Cookies);
                    return null;
                });
                
                next(app);
            };
        }

        private Response CreateRedirectionResponse(string redirectTo)
        {
            return new Response
            {
                Resource = new Json
                {
                    ["Html"] = _authenticationUriProvider.RedirectionViewUri,
                    ["RedirectUrl"] = HttpUtility.UrlDecode(redirectTo)
                }
            };
        }

        private void RegisterHtmlPage(string uri, string content)
        {
            Handle.GET(uri,
                () => {
                    var response = Response.FromStatusCode((int) HttpStatusCode.OK, content);
                    response.ContentType = "text/html";
                    return response;
                });
        }
    }
}