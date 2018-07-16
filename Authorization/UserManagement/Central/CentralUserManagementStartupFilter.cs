using System;
using Starcounter.Authorization.Model;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.UserManagement.Central
{
    internal class CentralUserManagementStartupFilter<TUser> : IStartupFilter
        where TUser : class, IMinimalUser
    {
        private readonly IUserManagementUriProvider<TUser> _userManagementUriProvider;

        public CentralUserManagementStartupFilter(IUserManagementUriProvider<TUser> userManagementUriProvider)
        {
            _userManagementUriProvider = userManagementUriProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                Handle.GET(_userManagementUriProvider.EditUserUriTemplate,
                    (string id) => new Json(),
                    new HandlerOptions()
                    {
                        SelfOnly = true
                    });
                Blender.MapUri2<TUser>(_userManagementUriProvider.EditUserUriTemplate);

                next(app);
            };
        }
    }
}