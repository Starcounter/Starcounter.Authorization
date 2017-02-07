using Starcounter.Advanced;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    // IBindable is to fake a db object
    public class Thing : IBindable
    {
        public int Id { get; set; }
        public ulong Identity { get; set; }
        public IBindableRetriever Retriever { get; set; }
    }
}