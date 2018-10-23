using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Settings;
using Starcounter.Authorization.SignIn;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Authentication
{
    internal class CleanupStartupFilter<TAuthenticationTicket> : IStartupFilter, IDisposable
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly IOptions<AuthorizationOptions> _options;
        private readonly ILogger<CleanupStartupFilter<TAuthenticationTicket>> _logger;
        private Timer _timer;

        public CleanupStartupFilter(IOptions<AuthorizationOptions> options,
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            ILogger<CleanupStartupFilter<TAuthenticationTicket>> logger)
        {
            _logger = logger;
            _authenticationTicketService = authenticationTicketService;
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _timer?.Dispose();
                var ticketCleanupInterval = _options.Value.TicketCleanupInterval;
                _timer = new Timer(CleanUp,
                    null, // state
                    TimeSpan.Zero, // initial delay
                    ticketCleanupInterval != TimeSpan.Zero // interval
                        ? ticketCleanupInterval
                        : TimeSpan.FromMilliseconds(-1));
                next(app);
            };
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void CleanUp(object state)
        {
#pragma warning disable CS0618 // No reason to use Task here
            Scheduling.ScheduleTask(() =>
            {
                try
                {
                    _authenticationTicketService.CleanExpiredTickets();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occured while cleaning expired tickets");
                }
            });
#pragma warning restore CS0618
        }
    }
}