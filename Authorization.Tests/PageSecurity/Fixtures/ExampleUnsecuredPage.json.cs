using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Tests.Fixtures;

namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures
{
    public partial class ExampleUnsecuredPage : Json, IExamplePage
    {
        public string Changed { get; set; }

        private void Handle(Input.Action1 action)
        {
            Changed = nameof(Action1);
        }

        [ExampleDataPage_json.PropertyTwo]
        public partial class ElementItem : IBound<ThingItem>, IExamplePage
        {
            public string Changed { get; set; }
            private void Handle(Input.SomeProperty action)
            {
                Changed = nameof(SomeProperty);
            }

            [Authorize(Roles = Roles.SpecificThingEditor)]
            private void Handle(Input.SomeSecuredProperty action)
            {
                Changed = nameof(SomeSecuredProperty);
            }

            [ExampleDataPage_json.PropertyTwo.NestedElements]
            public partial class NestedItem : IBound<OtherThingItem>, IExamplePage
            {
                public string Changed { get; set; }

                [Authorize(Roles = Roles.SpecificThingEditor)]
                private void Handle(Input.SomeSecuredNestedProperty action)
                {
                    Changed = nameof(SomeSecuredNestedProperty);
                }
            }
        }
    }
}
