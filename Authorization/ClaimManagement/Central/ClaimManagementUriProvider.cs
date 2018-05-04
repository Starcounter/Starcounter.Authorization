using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    public class ClaimManagementUriProvider<TClaimDb> : IClaimManagementUriProvider<TClaimDb>
        where TClaimDb : IClaimDb
    {
        public string EditClaimUriTemplate => $"/{Application.Current.Name}/Authorization.ClaimManagement.Master/{{?}}";
        public string CreateEditClaimUri(TClaimDb claimDb)
        {
            return EditClaimUriTemplate.Replace("{?}", claimDb.GetObjectID());
        }
    }
}