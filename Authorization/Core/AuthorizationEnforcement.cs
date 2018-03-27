using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Authentication;

namespace Starcounter.Authorization.Core
{
    public class AuthorizationEnforcement : IAuthorizationEnforcement
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationBackend _authentication;

        public AuthorizationEnforcement(IAuthorizationService authorizationService, IAuthenticationBackend authentication)
        {
            _authorizationService = authorizationService;
            _authentication = authentication;
        }

        public async Task<bool> CheckPolicyAsync(string policyName, object resource)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(_authentication.GetCurrentPrincipal(), resource, policyName);
            return authorizationResult.Succeeded;
        }

        public async Task<bool> CheckRequirementsAsync(IEnumerable<IAuthorizationRequirement> requirements, object resource)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(_authentication.GetCurrentPrincipal(), resource, requirements);
            return authorizationResult.Succeeded;
        }
    }
}