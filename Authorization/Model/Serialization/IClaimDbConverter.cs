using System;
using System.Security.Claims;

namespace Starcounter.Authorization.Model.Serialization
{
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public interface IClaimDbConverter
    {
        Claim Unpack(IClaimTemplate claimTemplate);
        void Pack(Claim claim, IClaimTemplate target);
    }
}