﻿using System.Web;

namespace Starcounter.Authorization.Authentication
{
    internal class AuthenticationUriProvider : IAuthenticationUriProvider
    {
        public string UnauthenticatedUriTemplate => $"/{Application.Current}/Starcounter.Authorization.Unauthenticated/{{?}}";

        public string SignOutUriTemplate => $"/{Application.Current}/Starcounter.Authorization.SignOut/{{?}}";

        public string SetTokenUriTemplate =>
            $"/{Application.Current}/Starcounter.Authorization.SetToken/{{?}}";

        public string UnauthenticatedViewUri =>
            $"/{Application.Current}/Starcounter.Authorization.Unauthenticated.html";

        public string UnauthorizedViewUri =>
            $"/{Application.Current}/Starcounter.Authorization.Unauthorized.html";

        public string RedirectionViewUri =>
            $"/{Application.Current}/Starcounter.Authorization.Redirection.html";

        public string CreateSetTokenUri(string destinationUri) => SetTokenUriTemplate.Replace("{?}", destinationUri);
        public string CreateSignOutUri(string destinationUri) => SignOutUriTemplate.Replace("{?}", destinationUri);
    }
}