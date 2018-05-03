using System;
using Starcounter.Authorization.Model;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    public class CentralClaimManagementStartupFilter<TClaimDb> : IStartupFilter
        where TClaimDb : class, IClaimDb
    {
        private readonly IClaimManagementUriProvider<TClaimDb> _claimManagementUriProvider;

        public CentralClaimManagementStartupFilter(IClaimManagementUriProvider<TClaimDb> claimManagementUriProvider)
        {
            _claimManagementUriProvider = claimManagementUriProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                Handle.GET(_claimManagementUriProvider.EditClaimUriTemplate,
                    (string id) => new Json(),
                    new HandlerOptions()
                    {
                        SelfOnly = true
                    });
                Blender.MapUri2<TClaimDb>(_claimManagementUriProvider.EditClaimUriTemplate);

                next(app);
            };
        }
    }
}