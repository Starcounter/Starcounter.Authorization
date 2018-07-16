using System.Collections.Generic;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class Group : IGroup
    {
        public IEnumerable<IClaimTemplate> AssociatedClaims { get; set; }
        public IEnumerable<IGroup> SubGroups { get; set; }
    }
}