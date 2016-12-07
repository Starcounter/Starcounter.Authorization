using Starcounter.Authorization.Attributes;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    [RequirePermission(typeof(ViewSpecificThing))]
    public partial class ExampleDataPage : Json, IBound<Thing>, IExamplePage
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

        [ExampleDataPage_json.Elements]
        public partial class ElementItem : IBound<ThingItem>
        {
            public string Changed { get; set; }
            private void Handle(Input.SomeProperty action)
            {
                Changed = nameof(SomeProperty);
            }
        }
    }
}
