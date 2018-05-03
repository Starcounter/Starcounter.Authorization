using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing.Activation;

namespace Starcounter.Authorization.ClaimManagement
{
    public static class ClaimManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Use this to register a view-model that will manage claims defined in your application.
        /// It will be automatically used by a user management application
        /// </summary>
        /// <param name="viewModelType">This should be a view-model that is IBound to <see cref="IClaimDb"/> and handles management for claims of type <paramref name="managedClaimType"/>.
        /// It shouldn't handle creating nor deleting the claim, only its population</param>
        /// <param name="services"></param>
        /// <param name="managedClaimType">The type of the claim to manage</param>
        /// <returns></returns>
        public static IServiceCollection AddClaimManagement<TClaimDb>(this IServiceCollection services,
            string managedClaimType,
            Type viewModelType)
            where TClaimDb : class, IClaimDb
        {
            services.TryAddTransient<IClaimDbConverter, ClaimDbConverter>();
            services.TryAddTransient<IPageCreator, DefaultPageCreator>();
            services.AddTransient<IStartupFilter>(provider => new ClaimManagementStartupFilter<TClaimDb>(
                managedClaimType,
                viewModelType,
                provider.GetService<ILogger<ClaimManagementStartupFilter<TClaimDb>>>()));
            return services;
        }

        /// <summary>
        /// Use this to register a view-model that will manage claims defined in your application.
        /// It will be automatically used by a user management application
        /// </summary>
        /// <typeparam name="TViewModel">This should be a view-model that is IBound to <see cref="IClaimDb"/> and handles management for claims of type <paramref name="managedClaimType"/>.
        /// It shouldn't handle creating nor deleting the claim, only its population</typeparam>
        /// <typeparam name="TClaimDb"></typeparam>
        /// <param name="services"></param>
        /// <param name="managedClaimType">The type of the claim to manage</param>
        /// <returns></returns>
        public static IServiceCollection AddClaimManagement<TClaimDb, TViewModel>(this IServiceCollection services,
            string managedClaimType)
            where TClaimDb : class, IClaimDb
        {
            return services.AddClaimManagement<TClaimDb>(managedClaimType, typeof(TViewModel));
        }

    }
}