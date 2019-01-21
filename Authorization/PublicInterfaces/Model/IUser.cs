using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Implement this interface with a database UserGroup class specific to your application to enable user groups manipulation
    /// </summary>
    public interface IUser: IMinimalUser, IClaimHolder
    {
        IEnumerable<IGroup> MemberOf { get; }

        string Username { get; }
    }
}