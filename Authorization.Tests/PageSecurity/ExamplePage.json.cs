using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    [Authorize(Policy = Policies.ViewThing)]
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

        [Authorize(Policy = Policies.ChangeThing)]
        private void Handle(Input.ChangeThing action)
        {
            Changed = nameof(ChangeThing);
        }

        [Authorize(Policy = Policies.ViewSpecificThing)]
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

            [Authorize(Policy = Policies.ChangeThing)]
            private void Handle(Input.ChangeSecuredSubThing action)
            {
                Changed = nameof(ChangeSecuredSubThing);
            }
        }
    }
}
