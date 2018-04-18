using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class User : IUserWithGroups
    {
        public IEnumerable<IClaimDb> AssociatedClaims { get; set; }
        public IEnumerable<IUserGroup> Groups { get; set; }
    }
}