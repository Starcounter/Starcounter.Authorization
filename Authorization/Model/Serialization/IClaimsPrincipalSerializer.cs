using System.Security.Claims;

namespace Starcounter.Authorization.Model.Serialization
{
    public interface IClaimsPrincipalSerializer
    {
        ClaimsPrincipal Deserialize(string serialized);
        string Serialize(ClaimsPrincipal principal);
    }
}