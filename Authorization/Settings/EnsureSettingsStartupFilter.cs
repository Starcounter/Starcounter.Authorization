using System;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Settings
{
    internal class EnsureSettingsStartupFilter<TAuthenticationSettings> : IStartupFilter
        where TAuthenticationSettings : class, IAuthorizationSettings, new()
    {
        private readonly ISettingsService<TAuthenticationSettings> _settingsService;

        public EnsureSettingsStartupFilter(ISettingsService<TAuthenticationSettings> settingsService)
        {
            _settingsService = settingsService;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _settingsService.EnsureSettings();
                next(app);
            };
        }
    }
}