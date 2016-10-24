using System.Linq;
using Simplified.Ring1;
using Simplified.Ring2;

namespace Starcounter.Authorization.Core
{
    /// <summary>
    /// To be changed to PermissionSomebodyGroup
    /// </summary>
    [Database]
    public class RolePersonGroup : Relation
    {
        private static T FromKey<T>(string key)
        {
            return Db.SQL<T>($"select o from {typeof(T).FullName} o where o.Key = ?", key).First();
        }

        static RolePersonGroup()
        {
            Db.Transact(() => {
                var role = OrganizationKiller.ForOrganization(FromKey<Organization>("Bn"));
                var rolePersonGroups =
                    Db.SQL<RolePersonGroup>(
                        $"select a from {typeof(RolePersonGroup).FullName} a where a.\"Group\".Key = ? and a.Role = ?", "FG",
                        role);
                if (!rolePersonGroups.Any())
                {
                    new RolePersonGroup() { Group = FromKey<SomebodyGroup>("FG"), Role = role };
                }
            });
        }

        [SynonymousTo(nameof(WhatIs))]
        public SomebodyGroup Group;

        [SynonymousTo(nameof(ToWhat))]
        public Role Role;
    }
}