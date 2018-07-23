using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Starcounter.Authorization.PageSecurity
{
    internal class AttributeRequirementsResolver : IAttributeRequirementsResolver
    {
        private readonly IAuthorizationPolicyProvider _policyProvider;

        public AttributeRequirementsResolver(IAuthorizationPolicyProvider policyProvider)
        {
            _policyProvider = policyProvider;
        }
        public async Task<IEnumerable<IAuthorizationRequirement>> ResolveAsync(IEnumerable<AuthorizeAttribute> attributes)
        {
            attributes = attributes as IList<AuthorizeAttribute> ?? attributes.ToList();
            var requirements = new List<IAuthorizationRequirement>();

            requirements.AddRange(GatherRoleRequirements(attributes));
            requirements.AddRange(await GatherPolicyRequirementsAsync(attributes));
            if (attributes.Any(attribute => attribute.Policy == null && attribute.Roles == null))
            {
                requirements.Add(new DenyAnonymousAuthorizationRequirement());
            }
            return requirements;
        }

        private static IEnumerable<IAuthorizationRequirement> GatherRoleRequirements(IEnumerable<AuthorizeAttribute> attributes)
        {
            return attributes
                .Select(attribute => attribute.Roles)
                .Where(roles => roles != null)
                .Select(roles => new RolesAuthorizationRequirement(roles.Split(',')));
        }

        private async Task<IEnumerable<IAuthorizationRequirement>> GatherPolicyRequirementsAsync(IEnumerable<AuthorizeAttribute> attributes)
        {
            var policies = await Task.WhenAll(attributes
                .Select(attribute => attribute.Policy)
                .Where(policyName => policyName != null)
                .Select(GetPolicyOrThrowAsync));
            return policies
                .SelectMany(policy => policy.Requirements);
        }

        private async Task<AuthorizationPolicy> GetPolicyOrThrowAsync(string policyName)
        {
            var policy = await _policyProvider.GetPolicyAsync(policyName);
            return policy ?? throw new InvalidOperationException($"The AuthorizationPolicy named: '{policyName}' was not found.");
        }
    }
}