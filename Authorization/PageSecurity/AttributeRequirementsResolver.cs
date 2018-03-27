using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Starcounter.Authorization.PageSecurity
{
    public class AttributeRequirementsResolver : IAttributeRequirementsResolver
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
            requirements.AddRange(attributes
                .Select(attribute => attribute.Roles)
                .Where(roles => roles != null)
                .Select(roles => new RolesAuthorizationRequirement(roles.Split(','))));
            var policies = await Task.WhenAll(attributes
                .Select(attribute => attribute.Policy)
                .Where(policy => policy != null)
                .Select(policy => _policyProvider.GetPolicyAsync(policy)));
            requirements.AddRange(policies.SelectMany(policy => policy.Requirements));
            if (attributes.Any(attribute => attribute.Policy == null && attribute.Roles == null))
            {
                requirements.Add(new DenyAnonymousAuthorizationRequirement());
            }
            return requirements;
        }
    }
}