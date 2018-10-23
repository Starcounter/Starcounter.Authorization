using System;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.SignIn;

namespace Starcounter.Authorization.Settings
{
    internal class OptionsProvider<TAuthorizationSettings> : IOptions<AuthorizationOptions>
        where TAuthorizationSettings : class, IAuthorizationSettings, new()
    {
        private readonly ISettingsService<TAuthorizationSettings> _settingsService;

        public OptionsProvider(ISettingsService<TAuthorizationSettings> settingsService)
        {
            _settingsService = settingsService;
        }

        public AuthorizationOptions Value
        {
            get {
                var settings = _settingsService.GetSettings();

                return new AuthorizationOptions
                {
                    TicketCleanupInterval = TimeSpan.FromSeconds(settings.TicketCleanupIntervalSeconds),
                    NewTicketExpiration = TimeSpan.FromSeconds(settings.NewTicketExpirationSeconds)
                };
            }
        }
    }
}