using System;
using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Authorization.Core
{
    // AuthorizationEnforcement?
    public static partial class AuthorizationRules
    {
        public static bool Check<TPermission, T>(T obj) where TPermission : Permission<T>
        {
            List<Claim> claims = new List<Claim>();
            var somebody = Authentication.GetCurrentPerson();
            if (somebody != null)
            {
                claims.Add(new PersonClaim(somebody));
            }

            return GetPredicatesForPermission<TPermission>()
                .Cast<Func<IEnumerable<Claim>, T, bool>>()
                .Any(p => p(claims, obj));
        }

        public static bool Check<TPermission>() where TPermission : Permission<Unit>
        {
            return Check<TPermission, Unit>(null);
        }
    }

    public static partial class AuthorizationRules
    {
        private static readonly Dictionary<Type, List<object>> Predicates = new Dictionary<Type, List<object>>();

        public static void Register<TPermission, TClaim, T>(Func<TClaim, T, bool> predicate) where TPermission : Permission<T> where TClaim : Claim
        {
            Register<TPermission, T>((claims, obj) => {
                var typedClaim = claims.OfType<TClaim>().FirstOrDefault();
                return typedClaim != null && predicate(typedClaim, obj);
            });
        }

        public static void Register<TPermission, TClaim, T>() where TPermission : Permission<T> where TClaim : Claim
        {
            Register<TPermission, TClaim, T>((claim, _) => true);
        }

        public static void Register<TPermission, T>(Func<IEnumerable<Claim>, T, bool> predicate) where TPermission : Permission<T>
        {
            GetPredicatesForPermission<TPermission>()
                .Add(predicate);
        }
        
        private static List<object> GetPredicatesForPermission<TPermission>()
        {
            List<object> p;
            if (!Predicates.TryGetValue(typeof(TPermission), out p))
            {
                p = new List<object>();
                Predicates[typeof(TPermission)] = p;
            }
            return p;
        }
    }
}