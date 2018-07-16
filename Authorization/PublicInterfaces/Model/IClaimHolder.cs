using System.Collections.Generic;

namespace Starcounter.Authorization
{
    public interface IClaimHolder
    {
        IEnumerable<IClaimTemplate> AssociatedClaims { get; }
    }
}