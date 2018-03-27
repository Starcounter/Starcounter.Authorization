using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Routing;

namespace StarcounterApplication1
{
    public class SystemUserAuthBackend : IAuthenticationBackend
    {
        public ClaimsPrincipal GetCurrentPrincipal()
        {
            var currentSystemUser = Db.Transact(SystemUser.GetCurrentSystemUser);
            IEnumerable<Claim> claims;
            if (currentSystemUser != null)
            {
                claims = new[] {new Claim(ClaimsIdentity.DefaultNameClaimType, currentSystemUser.Username),};
            }
            else
            {
                claims = Enumerable.Empty<Claim>();
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }

    public static class Extensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthenticationBackend, SystemUserAuthBackend>();
            return services;
        }
    }

    public static class StarcounterBootstrap
    {
        public static void Bootstrap()
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            var services = new ServiceCollection()
                // general
                .AddLogging(logging => logging.AddConsole())
                .AddOptions()
                // ms auth
                .AddAuthorization(auth => auth.AddPolicy("DoThings", builder => builder.RequireAuthenticatedUser()))
                // Starcounter
                .AddRouter()
                .AddAuthentication()
                .AddSecurityMiddleware(options => options.WithUnauthorizedAction(info => 400));

            var router = services.GetRouter();

            router.RegisterAllFromCurrentAssembly();
        }
    }


    public class Program
    {
        static void Main()
        {
            Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o");
            Console.WriteLine("Everything is OK!");
//            StarcounterBootstrap.Bootstrap();
        }
    }
}