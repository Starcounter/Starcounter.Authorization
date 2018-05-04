using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;
using Starcounter.Startup.Routing.Activation;

namespace Starcounter.Authorization.ClaimManagement
{
    public class ClaimManagementStartupFilter<TClaimDb> : IStartupFilter
        where TClaimDb: class, IClaimDb
    {
        private readonly string _claimType;
        private readonly Type _viewModelType;
        private readonly ILogger _logger;
        private readonly IPageCreator _pageCreator;

        public ClaimManagementStartupFilter(string claimType,
            Type viewModelType,
            ILogger<ClaimManagementStartupFilter<TClaimDb>> logger,
            IPageCreator pageCreator)
        {
            _claimType = claimType;
            _viewModelType = viewModelType;
            _logger = logger;
            _pageCreator = pageCreator;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                var uriTemplate = $"/{Application.Current}/Starcounter.Authorization.ClaimManagement/{_claimType}/{{?}}";
                Blender.MapUri2<TClaimDb>(uriTemplate);
                Handle.GET(uriTemplate,
                    (string id, Request request) => {
                        var claimDb = Db.FromId<TClaimDb>(id);
                        if (claimDb == null)
                        {
                            _logger.LogWarning(
                                "Received request for claim with ID '{0}', which does not exist. Ignoring",
                                id);
                            return new Json();
                        }
                        // this will be blended to every claim edited, but we want to edit only ours
                        if (claimDb.Type != _claimType)
                        {
                            return new Json();
                        }

                        return _pageCreator.Create(new RoutingInfo()
                        {
                            Request = request,
                            SelectedPageType = _viewModelType,
                            Context = claimDb,
                            Arguments = new string[0]
                        });
                    },
                    new HandlerOptions()
                    {
                        SelfOnly = true
                    });
                next(app);
            };
        }
    }
}