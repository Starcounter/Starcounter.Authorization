using System.Collections.Generic;

namespace Starcounter.Authorization.Core.Rules
{
    /// <summary>
    /// One rule to determine whether a user is granted a permission
    /// </summary>
    /// <typeparam name="TPermission">type of permission to be checked</typeparam>
    public interface IAuthorizationRule<in TPermission> where TPermission : Permission
    {
        /// <summary>
        /// Evaluate the rule
        /// </summary>
        /// <param name="claims">Collection of claims about current user. They are the source of information about the current user</param>
        /// <param name="authorizationEnforcement">Mean to query about other permissions. 
        /// Can be used to grant permissions basing only on the fact that some other permission would be granted</param>
        /// <param name="permission">The permission to check</param>
        /// <returns>true if permission should be granted, false otherwise</returns>
        bool Evaluate(IEnumerable<Claim> claims, IAuthorizationEnforcement authorizationEnforcement, TPermission permission);
    }
}