using System.Collections.Generic;

namespace Starcounter.Authorization.Model
{
    /// <summary>
    /// Implement this interface with a database UserGroup class specific to your application to enable user groups manipulation
    /// </summary>
    public interface IUserGroup
    {
        IEnumerable<IClaimDb> AssociatedClaims { get; }
        IEnumerable<IUserGroup> SubGroups { get; }
    }
}