namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures
{
    public static class Roles
    {
        public const string ThingViewer = nameof(ThingViewer);
        public const string ThingEditor = nameof(ThingEditor);
        public const string SpecificThingViewer = nameof(SpecificThingViewer); // uses Thing as resource
        public const string SpecificThingEditor = nameof(SpecificThingEditor); // uses Thing as resource
    }
}