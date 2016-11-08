namespace Starcounter.Authorization.Core.Rules
{
    /// <summary>
    /// Implementers allow for configuration of authorization rules.
    /// </summary>
    public interface IAuthorizationConfigurator
    {
        /// <summary>
        /// Add an authorization rule for given permission. Multiple rules for the same permission are logically
        /// summed (any of them passing grant access)
        /// </summary>
        /// <typeparam name="TPermission">Type of the permission that is configured</typeparam>
        /// <param name="rule">The rule deciding if access should be granted</param>
        void AddRule<TPermission>(IAuthorizationRule<TPermission> rule)
            where TPermission : Permission;
    }
}