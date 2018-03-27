using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.Tests.TestUtils
{
    public class EmptyPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            throw new NotImplementedException();
        }
    }
}