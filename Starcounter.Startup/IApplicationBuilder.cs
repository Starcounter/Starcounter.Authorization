using System;

namespace Starcounter.Startup
{
    public interface IApplicationBuilder
    {
        IServiceProvider ApplicationServices { get; set; }
    }
}