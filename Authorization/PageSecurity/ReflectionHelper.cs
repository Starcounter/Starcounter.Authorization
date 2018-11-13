using System;
using System.Reflection;

namespace Starcounter.Authorization.PageSecurity
{
    internal static class ReflectionHelper
    {
        public static object InvokePrivateGenericMethod(object @this, string name, Type[] typeParameter, params object[] arguments)
        {
            return @this.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeParameter)
                .Invoke(@this, arguments);
        }
    }
}