using System;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.Authentication
{
    internal class TicketCreationStartupFilter : IStartupFilter
    {
        private readonly IAuthCookieService _authCookieService;

        public TicketCreationStartupFilter(
            IAuthCookieService authCookieService)
        {
            _authCookieService = authCookieService;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                Application.Current.Use(request =>
                {
                    if (request.IsStaticFileRequest)
                    {
                        return null;
                    }
                    Session.Ensure();
                    _authCookieService.ReattachOrCreate(request.Cookies);
                    return null;
                });
                next(app);
            };
        }
    }
}