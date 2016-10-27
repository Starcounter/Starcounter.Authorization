using System.Collections.Generic;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Authentication
{
    //    // AuthorizationEnforcement?
    //    public static partial class AuthorizationRules
    //    {
    //        public static bool Check<TPermission, T>(T obj) where TPermission : Permission<T>
    //        {
    //            List<Claim> claims = new List<Claim>();
    //            var somebody = Authentication.GetCurrentPerson();
    //            if (somebody != null)
    //            {
    //                claims.Add(new PersonClaim(somebody));
    //            }
    //
    //            return GetPredicatesForPermission<TPermission>()
    //                .Cast<Func<IEnumerable<Claim>, T, bool>>()
    //                .Any(p => p(claims, obj));
    //        }
    //
    //        public static bool Check<TPermission>() where TPermission : Permission<Unit>
    //        {
    //            return Check<TPermission, Unit>(null);
    //        }
    //    }
    public interface IAuthenticationBackend
    {
        IEnumerable<Claim> GetCurrentClaims();
    }
}