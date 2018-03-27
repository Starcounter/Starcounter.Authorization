using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.Core
{
    public interface IAuthorizationEnforcement
    {
        Task<bool> CheckRequirementsAsync(IEnumerable<IAuthorizationRequirement> requirements, object resource);
    }
}