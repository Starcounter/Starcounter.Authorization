using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization.ClaimManagement.Central
{
    /// <summary>
    /// Use this to enable managing claims from different applications. This method registers <see cref="IClaimManagementUriProvider{TClaimDb}"/>
    /// </summary>
    [Obsolete("Don't use this interface. It will be made internal soon")]
    public static class CentralClaimsManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddCentralClaimsManagement<TClaimDb>(this IServiceCollection services)
            where TClaimDb : class, IClaimTemplate
        {
            services.AddTransient<IClaimManagementUriProvider<TClaimDb>, ClaimManagementUriProvider<TClaimDb>>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, CentralClaimManagementStartupFilter<TClaimDb>>());
            services.AddTransient<IClaimDbConverter, ClaimDbConverter>();
            return services;
        }
    }
}