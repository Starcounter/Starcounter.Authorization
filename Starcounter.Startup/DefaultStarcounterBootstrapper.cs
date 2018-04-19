using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Starcounter.Startup
{
    public class DefaultStarcounterBootstrapper
    {
        public static void Start(IStartup application)
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            var services = new ServiceCollection()
                .AddOptions()
                .AddLogging(logging => logging.AddConsole());
            application.ConfigureServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<DefaultStarcounterBootstrapper>>();
            logger.LogInformation($"Configuring application {application}");
            var applicationBuilder = new ApplicationBuilder()
            {
                ApplicationServices = serviceProvider
            };
            Action<IApplicationBuilder> configure = application.Configure;
            foreach (var startupFilter in serviceProvider.GetServices<IStartupFilter>().Reverse())
            {
                configure = startupFilter.Configure(configure);
            }

            configure(applicationBuilder);
            logger.LogInformation($"Started application {application}");
        }
    }
}
