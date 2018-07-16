using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    internal class ClaimManagementUriProvider<TClaimDb> : IClaimManagementUriProvider<TClaimDb>
        where TClaimDb : IClaimTemplate
    {
        public string EditClaimUriTemplate => $"/{Application.Current.Name}/Authorization.ClaimManagement.Master/{{?}}";
        public string CreateEditClaimUri(TClaimDb claimTemplate)
        {
            return EditClaimUriTemplate.Replace("{?}", claimTemplate.GetObjectID());
        }
    }
}