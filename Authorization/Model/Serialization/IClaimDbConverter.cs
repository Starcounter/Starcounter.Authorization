using System.Security.Claims;

namespace Starcounter.Authorization.Model.Serialization
{
    public interface IClaimDbConverter
    {
        Claim Unpack(IClaimDb claimDb);
        void Pack(Claim claim, IClaimDb target);
    }
}