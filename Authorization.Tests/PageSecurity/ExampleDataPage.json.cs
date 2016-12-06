using Starcounter.Authorization.Attributes;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    [RequirePermission(typeof(ViewSpecificThing))]
    partial class ExampleDataPage : Json, IBound<Thing>, IExamplePage
    {
        public string Changed { get; set; }

        private void Handle(Input.Action1 action)
        {
            Changed = nameof(Action1);
        }

        [RequirePermission(typeof(ChangeThing))]
        private void Handle(Input.Action2 action)
        {
            Changed = nameof(Action2);
        }
    }
}
