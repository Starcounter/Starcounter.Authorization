using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class UserGroup : IUserGroup
    {
        public IEnumerable<IClaimDb> AssociatedClaims { get; set; }
        public IEnumerable<IUserGroup> SubGroups { get; set; }
    }
}