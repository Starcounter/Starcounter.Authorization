using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class ViewSpecificThing : Permission
    {
        public Thing Thing { get; set; }

        public ViewSpecificThing(Thing thing)
        {
            Thing = thing;
        }
    }
}