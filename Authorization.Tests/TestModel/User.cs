using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class User : IUser
    {
        public IEnumerable<IClaimTemplate> AssociatedClaims { get; set; }
        public IEnumerable<IGroup> MemberOf { get; set; }
    }
}