using System.Linq;
using Simplified.Ring2;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Examples
{
    public class ViewOrganizationsPermission : Permission<Unit> { }

    public class DeleteOrganizationPermission : Permission<Organization>
    {
    }

    public class ViewOrganizationPermission : Permission<Organization>
    {
    }

    public class SetupExample
    {
        public void Setup()
        {
            AuthorizationRules.Register<ViewOrganizationsPermission, PersonClaim, Unit>();
            AuthorizationRules.Register<ViewOrganizationPermission, PersonClaim, Organization>((claim, organization) => claim.Person.FirstName == "admin");
            AuthorizationRules.Register<DeleteOrganizationPermission, PersonClaim, Organization>((claim, organization) => claim.Person.FirstName == "admin");
            AuthorizationRules.Register<ViewOrganizationPermission, PersonClaim, Organization>((claim, organization) => organization.Members.Contains(claim.Person));
            AuthorizationRules.Register<DeleteOrganizationPermission, PersonClaim, Organization>((claim, organization) => {
                var entitledGroups = Db.SQL<SomebodyGroup>($"select rpg.\"Group\" from {typeof(RolePersonGroup).FullName} rpg join {typeof(OrganizationKiller).FullName} ok on rpg.Role = ok and ok.Organization = ?", organization);
                return entitledGroups.Any(group => group.GetAllMembers().Contains(claim.Person));
            }
            );
        }
    }
}