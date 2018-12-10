using System.Collections.Generic;
using System.Linq;
using Starcounter;
using Starcounter.Authorization;
using Starcounter.Linq;

namespace Application
{
    [Database]
    public class User : ClaimHolder, IUser
    {
        public IEnumerable<UserGroup> MemberOf =>
            DbLinq.Objects<UserGroupMember>()
                .Where(member => member.WhatIs == this)
                .Select(item => item.ToWhat)
                .ToList();

        IEnumerable<IGroup> IUser.MemberOf => MemberOf;
    }
}