using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Lets anyone bearing <see cref="CommonClaims.SuperuserClaimType"/> claim do anything.
    /// Add it in your Startup.Configure method with <code><![CDATA[services.TryAddTransient<IAuthorizationHandler, SuperuserAuthorizationHandler>()]]></code>
    /// </summary>
    public class SuperuserAuthorizationHandler: IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(claim => claim.Type == CommonClaims.SuperuserClaimType))
            {
                foreach (var requirement in context.PendingRequirements)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}