using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Starcounter.Authorization
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        private static readonly AssertionRequirement AllowAnonymousRequirement = new AssertionRequirement(context => true);

        /// <summary>
        /// Adds an <see cref="T:Microsoft.AspNetCore.Authorization.Infrastructure.AssertionRequirement" /> that will always suceed to the current instance.
        /// Using this method in policy configuration makes the policy a no-op. Use it when you already have a policy structure, but don't want to check any
        /// rules yet.
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        public static void AllowAnonymous(this AuthorizationPolicyBuilder builder)
        {
            builder.AddRequirements(AllowAnonymousRequirement);
        }
    }
}