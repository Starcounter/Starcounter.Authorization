using Starcounter.Authorization.Attributes;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    [RequirePermission(typeof(ViewSpecificThing))]
    public partial class ExampleDataPage : Json, IBound<Thing>
    {
        private void Handle(Input.Action1 action)
        {
            
        }

        [RequirePermission(typeof(ChangeThing))]
        private void Handle(Input.Action2 action)
        {
            
        }

        [ExampleDataPage_json.Elements]
        public partial class ElementItem
        {
            public string Changed { get; set; }
            private void Handle(Input.SomeProperty action)
            {
                Changed = nameof(SomeProperty);
            }
        }
    }
}
