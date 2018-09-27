using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Linq;
using Starcounter.Startup.Abstractions;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Creates ClaimTemplate for a specified claim type, but only if it doesn't exist yet. One instance of this type creates one claim template.
    /// </summary>
    /// <typeparam name="TClaimTemplate"></typeparam>
    internal class ClaimCreatorStartupFilter<TClaimTemplate> : IStartupFilter
        where TClaimTemplate : IClaimTemplate, new()
    {
        private readonly IClaimDbConverter _claimDbConverter;
        private readonly ILogger _logger;
        private readonly string _claimType;

        public ClaimCreatorStartupFilter(IClaimDbConverter claimDbConverter, 
            ILogger<ClaimCreatorStartupFilter<TClaimTemplate>> logger,
            string claimType)
        {
            _claimDbConverter = claimDbConverter;
            _logger = logger;
            _claimType = claimType;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var shouldCreateClaim = DbLinq.Objects<TClaimTemplate>()
                                            .FirstOrDefault(template => template.Type == _claimType) == null;
                if (shouldCreateClaim)
                {
                    _logger.LogInformation($"Creating new ClaimTemplate for type {_claimType}");
                    Db.Transact(() =>
                    {
                        var claimTemplate = new TClaimTemplate();
                        _claimDbConverter.Pack(new Claim(_claimType, String.Empty), claimTemplate);
                    });
                }
                else
                {
                    _logger.LogInformation($"ClaimTemplate for type {_claimType} already exists");
                }

                next(app);
            };
        }
    }
}