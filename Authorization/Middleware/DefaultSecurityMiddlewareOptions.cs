﻿using System;
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
            var json = new Json
            {
                ["Html"] = _authenticationUriProvider.UnauthorizedViewUri
            };

            var response = new Response()
            {
                Resource = json,
                StatusCode = (int) HttpStatusCode.NotFound,
                StatusDescription = "Not Found"
            };

            return response;
        }
    }
}