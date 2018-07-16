using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Implement this interface with a database UserGroup class specific to your application to enable user groups manipulation
    /// </summary>
    public interface IGroup: IClaimHolder
    {
        IEnumerable<IGroup> SubGroups { get; }
    }
}