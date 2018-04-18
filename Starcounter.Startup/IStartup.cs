using Microsoft.Extensions.DependencyInjection;

namespace Starcounter.Startup
{
    public interface IStartup
    {
        IServiceCollection ConfigureServices(IServiceCollection services);
        void Configure(IApplicationBuilder applicationBuilder);
    }
}