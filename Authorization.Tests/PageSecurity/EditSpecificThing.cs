using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class EditSpecificThing : Permission
    {
        public Thing Thing { get; set; }

        public EditSpecificThing(Thing thing)
        {
            Thing = thing;
        }
    }
}