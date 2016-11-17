using Starcounter.Authorization.Attributes;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    [RequirePermission(typeof(ViewSpecificThing))]
    partial class ExampleDataPage : Json, IBound<Thing>
    {
        private void Handle(Input.Action1 action)
        {
            
        }

        [RequirePermission(typeof(ChangeThing))]
        private void Handle(Input.Action2 action)
        {
            
        }
    }
}
