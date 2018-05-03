using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    public interface IClaimManagementUriProvider<in TClaimDb>
        where TClaimDb : IClaimDb
    {
        string EditClaimUriTemplate { get; }
        string CreateEditClaimUri(TClaimDb claimDb);
    }
}