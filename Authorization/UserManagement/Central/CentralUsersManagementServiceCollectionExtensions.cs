using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.UserManagement.Central
{
    /// <summary>
    /// Use this to enable managing claims from different applications. This method registers <see cref="IUserManagementUriProvider{TUserDb}"/>
    /// </summary>
    public static class CentralUsersManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddCentralUsersManagement<TUser>(this IServiceCollection services)
            where TUser : class, IUser
        {
            services.AddTransient<IUserManagementUriProvider<TUser>, UserManagementUriProvider<TUser>>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, CentralUserManagementStartupFilter<TUser>>());
            return services;
        }
    }
}