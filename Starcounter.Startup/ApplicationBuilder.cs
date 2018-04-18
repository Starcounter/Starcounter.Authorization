using System;

namespace Starcounter.Startup
{
    public class ApplicationBuilder : IApplicationBuilder
    {
        public IServiceProvider ApplicationServices { get; set; }
    }
}