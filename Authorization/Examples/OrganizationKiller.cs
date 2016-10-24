using System.Linq;
using Simplified.Ring2;

namespace Starcounter.Authorization.Core
{
    public class OrganizationKiller : Role
    {
        public static OrganizationKiller ForOrganization(Organization organization)
        {
            return Db.SQL<OrganizationKiller>($"select ok from {typeof(OrganizationKiller)} ok where ok.Organization = ?", organization)
                       .FirstOrDefault() ?? new OrganizationKiller() { Organization = organization };
        }
        public Organization Organization { get; set; }
    }
}