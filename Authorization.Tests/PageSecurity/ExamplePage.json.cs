using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Routing;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class ChangeThing : Permission { }
    public class ViewThing : Permission { }

    [Url("/ChecklistDesigner/not-found")]
    [RequirePermission(typeof(ViewThing))]
    partial class ExamplePage : Json, IBound<Thing>
    {
        private void Handle(Input.ActionNotMarked action)
        {
            
        }

        [RequirePermission(typeof(ChangeThing))]
        private void Handle(Input.ChangeThing action)
        {
            
        }

        [RequirePermissionData(typeof(ViewSpecificThing))]
        private void Handle(Input.ViewSpecificThing action)
        {

        }
    }
}
