using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;
using Starcounter.Startup.Routing.Activation;

namespace Starcounter.Authorization.UserManagement
{
    public class UserManagementStartupFilter<TUser> : IStartupFilter
        where TUser: class, IUser
    {
        private readonly Type _viewModelType;
        private readonly ILogger _logger;

        public UserManagementStartupFilter(Type viewModelType,
            ILogger<UserManagementStartupFilter<TUser>> logger)
        {
            _viewModelType = viewModelType;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                var uriTemplate = $"/{Application.Current.Name}/Starcounter.Authorization.UserManagement/{{?}}";
                var pageCreator = app.ApplicationServices.GetRequiredService<IPageCreator>();
                Blender.MapUri2<TUser>(uriTemplate);
                Handle.GET(uriTemplate,
                    (string id, Request request) => {
                        var user = Db.FromId<TUser>(id);
                        if (user == null)
                        {
                            _logger.LogWarning(
                                "Received request for user with ID '{0}', which does not exist. Ignoring",
                                id);
                            return new Json();
                        }

                        return pageCreator.Create(new RoutingInfo()
                        {
                            Request = request,
                            SelectedPageType = _viewModelType,
                            Context = user,
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