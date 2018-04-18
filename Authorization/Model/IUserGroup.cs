using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    public interface IUserGroup
    {
        IEnumerable<IClaimDb> AssociatedClaims { get; }
        IEnumerable<IUserGroup> SubGroups { get; }
    }
}