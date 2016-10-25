using System.Linq;
using Simplified.Ring2;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Core.Rules;

namespace Starcounter.Authorization.Examples
{
    public class ViewOrganizationsPermission : Permission { }

    public class DeleteOrganizationPermission : Permission
    {
        public Organization Organization { get; private set; }

        public DeleteOrganizationPermission(Organization organization)
        {
            Organization = organization;
        }
    }

    public class ViewOrganizationPermission : Permission
    {
        public Organization Organization { get; private set; }

        public ViewOrganizationPermission(Organization organization)
        {
            Organization = organization;
        }
    }

    public class SetupExample
    {
        public void Setup()
        {
            var authConfig = AuthorizationStatic.Rules;
            authConfig.RequireClaim<ViewOrganizationsPermission, PersonClaim>();
            authConfig.AddClaimRule<ViewOrganizationPermission, PersonClaim>((claim, p) => claim.Person.FirstName == "admin");
            authConfig.AddClaimRule<ViewOrganizationPermission, PersonClaim>((claim, p) => p.Organization.Members.Contains(claim.Person));
            authConfig.AddClaimRule<DeleteOrganizationPermission, PersonClaim>((claim, p) => claim.Person.FirstName == "admin");
            authConfig.AddClaimRule<DeleteOrganizationPermission, PersonClaim>((claim, p) => {
                var entitledGroups = Db.SQL<SomebodyGroup>($"select rpg.\"Group\" from {typeof(RolePersonGroup).FullName} rpg join {typeof(OrganizationKiller).FullName} ok on rpg.Role = ok and ok.Organization = ?", p.Organization);
                return entitledGroups.Any(group => group.GetAllMembers().Contains(claim.Person));
            });
        }
    }
}