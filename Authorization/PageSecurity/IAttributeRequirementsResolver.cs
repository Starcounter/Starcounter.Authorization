using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.PageSecurity
{
    internal interface IAttributeRequirementsResolver
    {
        Task<IEnumerable<IAuthorizationRequirement>> ResolveAsync(IEnumerable<AuthorizeAttribute> attributes);
    }
}