using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    public interface IAssociatedToClaims
    {
        IEnumerable<IClaimDb> AssociatedClaims { get; }
    }
}