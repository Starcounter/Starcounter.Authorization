using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Linq;

namespace Starcounter.Authorization.Settings
{
    /// <inheritdoc />
    internal class SettingsService<TAuthorizationSettings> : ISettingsService<TAuthorizationSettings>
        where TAuthorizationSettings : class, IAuthorizationSettings, new()
    {
        private readonly ILogger<SettingsService<TAuthorizationSettings>> _logger;
        private readonly ITransactionFactory _transactionFactory;

        public SettingsService(ILogger<SettingsService<TAuthorizationSettings>> logger,
            ITransactionFactory transactionFactory)
        {
            _logger = logger;
            _transactionFactory = transactionFactory;
        }

        /// <inheritdoc />
        public TAuthorizationSettings GetSettings()
        {
            var settingsList = DbLinq.Objects<TAuthorizationSettings>().ToList();
            if (settingsList.Count != 1)
            {
                throw new InvalidOperationException($"There should be exactly 1 {typeof(TAuthorizationSettings)} objects, but found {settingsList.Count}");
            }

            return settingsList.First();
        }

        public TAuthorizationSettings EnsureSettings()
        {
            return _transactionFactory.ExecuteTransaction(() =>
            {
                var settingsList = DbLinq.Objects<TAuthorizationSettings>().ToList();
                if (settingsList.Count > 1)
                {
                    // If we decided to keep one instance it would be undeterministic
                    _logger.LogWarning($"{settingsList.Count} instances of {typeof(TAuthorizationSettings)} found. Removing all");
                    foreach (var settings in settingsList)
                    {
                        settings.Delete();
                    }
                    return CreateSettings();
                }

                if (!settingsList.Any())
                {
                    return CreateSettings();
                }

                return settingsList.First();
            });
        }

        private static TAuthorizationSettings CreateSettings()
        {
            return new TAuthorizationSettings
            {
                NewTicketExpirationSeconds = AuthorizationSettings.DefaultNewTicketExpirationSeconds,
                TicketCleanupIntervalSeconds = AuthorizationSettings.DefaultTicketCleanupIntervalSeconds
            };
        }
    }
}