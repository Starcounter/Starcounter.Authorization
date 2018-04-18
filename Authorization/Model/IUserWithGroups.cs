using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    public interface IUserWithGroups: IUser
    {
        IEnumerable<IClaimDb> AssociatedClaims { get; }
        IEnumerable<IUserGroup> Groups { get; }
    }
}