using System.Collections.Generic;
using System.Linq;
using Starcounter;
using Starcounter.Authorization;
using Starcounter.Linq;

namespace Application
{
    [Database]
    public class UserGroup : ClaimHolder, IGroup
    {
        public string Name { get; set; }

        public UserGroup Parent { get; set; }

        public IEnumerable<IGroup> SubGroups => DbLinq.Objects<UserGroup>()
            .Where(group => group.Parent == this);
    }
}