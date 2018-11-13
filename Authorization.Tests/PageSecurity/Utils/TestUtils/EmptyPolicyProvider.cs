using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils
{
    public class EmptyPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        }
    }
}