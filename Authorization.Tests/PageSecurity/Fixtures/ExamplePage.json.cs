using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures
{
    [Authorize(Roles = Roles.ThingViewer)]
    public partial class ExamplePage : Json, IBound<Thing>, IExamplePage
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

        [Authorize(Roles = Roles.ThingEditor)]
        private void Handle(Input.ChangeThing action)
        {
            Changed = nameof(ChangeThing);
        }

        [Authorize(Roles = Roles.SpecificThingViewer)]
        private void Handle(Input.ViewSpecificThing action)
        {
            Changed = nameof(ViewSpecificThing);
        }

        [AllowAnonymous]
        private void Handle(Input.PubliclyAccessibleThing action)
        {
            Changed = nameof(PubliclyAccessibleThing);
        }

        [ExamplePage_json.Elements]
        public partial class Element: Json, IExamplePage
        {
            public string Changed { get; set; }
            private void Handle(Input.ChangeSubThing action)
            {
                Changed = nameof(ChangeSubThing);
            }

            [Authorize(Roles = Roles.ThingEditor)]
            private void Handle(Input.ChangeSecuredSubThing action)
            {
                Changed = nameof(ChangeSecuredSubThing);
            }
        }
    }
}
