using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Starcounter.Authorization.Routing
{
    public static class RouterServiceCollectionExtensions
    {
        public static IServiceCollection AddRouter(this IServiceCollection services)
        {
            services.TryAddSingleton<IPageCreator, DefaultPageCreator>();
            services.TryAddTransient<Router>();

            return services;
        }

        public static Router GetRouter(this IServiceCollection services) =>
            services.BuildServiceProvider().GetRequiredService<Router>();

    }
}