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

        [ExampleDataPage_json.PropertyTwo]
        public partial class ElementItem : IBound<ThingItem>, IExamplePage
        {
            public string Changed { get; set; }
            private void Handle(Input.SomeProperty action)
            {
                Changed = nameof(SomeProperty);
            }

            [RequirePermission(typeof(EditSpecificThing))]
            private void Handle(Input.SomeSecuredProperty action)
            {
                Changed = nameof(SomeSecuredProperty);
            }

            [ExampleDataPage_json.PropertyTwo.NestedElements]
            public partial class NestedItem : IBound<OtherThingItem>, IExamplePage
            {
                public string Changed { get; set; }

                [RequirePermission(typeof(EditSpecificThing))]
                private void Handle(Input.SomeSecuredNestedProperty action)
                {
                    Changed = nameof(SomeSecuredNestedProperty);
                }
            }
        }
    }
}
