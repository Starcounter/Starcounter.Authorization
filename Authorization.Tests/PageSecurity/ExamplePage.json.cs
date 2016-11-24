using System.Collections.Generic;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class ChangeThing : Permission { }
    public class ViewThing : Permission { }

    [RequirePermission(typeof(ViewThing))]
    public partial class ExamplePage : Json, IBound<Thing>
    {
        public string Changed { get; set; }

        public void Init()
        {
            stateFlags = new List<PropertyState>();
        }

        private void Handle(Input.ActionNotMarked action)
        {
            Changed = nameof(ActionNotMarked);
        }

        [RequirePermission(typeof(ChangeThing))]
        private void Handle(Input.ChangeThing action)
        {
            Changed = nameof(ChangeThing);
        }

        [RequirePermission(typeof(ViewSpecificThing))]
        private void Handle(Input.ViewSpecificThing action)
        {
            Changed = nameof(ViewSpecificThing);
        }

        [ExamplePage_json.Elements]
        public partial class Element
        {
            public string Changed { get; set; }
            private void Handle(Input.ChangeSubThing action)
            {
                Changed = nameof(ChangeSubThing);
            }
        }
    }
}
