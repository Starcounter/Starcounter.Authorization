using Microsoft.Extensions.DependencyInjection;

namespace Starcounter.Startup
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);
        void Configure(IApplicationBuilder applicationBuilder);
    }
}