using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public static class Policies
    {
        public const string ViewThing = nameof(ViewThing);
        public const string ChangeThing = nameof(ChangeThing);
        public const string ViewSpecificThing = nameof(ViewSpecificThing);
        public const string EditSpecificThing = nameof(EditSpecificThing);
    }
}