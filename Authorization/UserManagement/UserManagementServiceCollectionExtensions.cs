using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing.Activation;

namespace Starcounter.Authorization.UserManagement
{
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public static class UserManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Use this to register a view-model that will manage those aspects of users that are defined in your application.
        /// It will be automatically used by a user management application
        /// </summary>
        /// <param name="viewModelType">This should be a view-model that is IBound to <see cref="IMinimalUser"/> and handles management for user
        /// It shouldn't handle creating nor deleting the claim, only its population</param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUserManagement<TUser>(this IServiceCollection services,
            Type viewModelType)
            where TUser : class, IMinimalUser
        {
            services.TryAddTransient<IPageCreator, DefaultPageCreator>();
            services.AddTransient<IStartupFilter>(provider => ActivatorUtilities.CreateInstance<UserManagementStartupFilter<TUser>>(
                provider,
                viewModelType));
            return services;
        }

        /// <summary>
        /// Use this to register a view-model that will manage claims defined in your application.
        /// It will be automatically used by a user management application
        /// </summary>
        /// <typeparam name="TViewModel">This should be a view-model that is IBound to <see cref="IMinimalUser"/> and handles management for user
        /// It shouldn't handle creating nor deleting the claim, only its population</typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUserManagement<TUser, TViewModel>(this IServiceCollection services)
            where TUser : class, IMinimalUser
        {
            return services.AddUserManagement<TUser>(typeof(TViewModel));
        }

    }
}