using System.Threading.Tasks;

namespace Starcounter.Authorization.Core
{
    public interface IAuthorizationEnforcement
    {
        Task<bool> CheckPolicyAsync(string policyName, object resource);
    }
}