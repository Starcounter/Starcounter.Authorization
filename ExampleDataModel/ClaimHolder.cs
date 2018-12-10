using System.Collections.Generic;
using System.Linq;
using Starcounter;
using Starcounter.Authorization;
using Starcounter.Linq;

namespace Application
{
    [Database]
    public abstract class ClaimHolder : IClaimHolder
    {
        public IEnumerable<IClaimTemplate> AssociatedClaims => DbLinq.Objects<ClaimRelation>()
            .Where(relation => relation.Subject == this)
            .Select(relation => relation.Object);
    }
}