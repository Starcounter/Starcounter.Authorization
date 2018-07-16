using System;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public interface IClaimManagementUriProvider<in TClaimTemplate>
        where TClaimTemplate : IClaimTemplate
    {
        string EditClaimUriTemplate { get; }

        /// <summary>
        /// Returns a URI that can be passed to <see cref="Self.GET(string)"/> to obtain blended "edit claim" partial from relevant application
        /// </summary>
        /// <param name="claimTemplate"></param>
        /// <returns></returns>
        string CreateEditClaimUri(TClaimTemplate claimTemplate);
    }
}