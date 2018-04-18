using System;

namespace Starcounter.Startup
{
    public interface IStartupFilter
    {
        Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> action);
    }
}