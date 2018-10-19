using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.SignIn;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Authentication
{
    internal class CleanupStartupFilter<TAuthenticationTicket> : IStartupFilter, IDisposable
        where TAuthenticationTicket : IScAuthenticationTicket
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly SignInOptions _options;
        private readonly ILogger<CleanupStartupFilter<TAuthenticationTicket>> _logger;
        private Timer _timer;

        public CleanupStartupFilter(IOptions<SignInOptions> options,
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            ILogger<CleanupStartupFilter<TAuthenticationTicket>> logger)
        {
            _logger = logger;
            _authenticationTicketService = authenticationTicketService;
            _options = options.Value;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _timer?.Dispose();
                _timer = new Timer(CleanUp, null, TimeSpan.Zero, _options.TicketCleanupInterval ?? TimeSpan.FromMilliseconds(-1));
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