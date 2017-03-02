using System;
using System.Linq;
using System.Reflection;

namespace Starcounter.Authorization.Routing
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// Returns a method from specified type, marked with specified attribute.
        /// If this method is not public and static or its return type or parameter type doesn't match the requirement
        /// or if it doesn't accept exactly one parameter, this method will throw an exception.
        /// </summary>
        /// <param name="pageType">The type in which the method is sought</param>
        /// <param name="attribute">The attribute with which the sought method should be marked</param>
        /// <param name="expectedParameterType">The required parameter type for the method.
        ///  If the method doesn't accept exactly one parameter it throws an exception</param>
        /// <param name="expectedReturnType">The required return type for the method</param>
        /// <returns>The found method handle or null if no method with specified attribute exists in specified type</returns>
        public static MethodInfo GetStaticMethodWithAttribute(
            Type pageType,
            Type attribute,
            Type expectedParameterType,
            Type expectedReturnType)
        {
            try
            {
                var methodsWithAttribute = pageType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(method => method.GetCustomAttribute(attribute) != null)
                    .ToList();
                if (methodsWithAttribute.Count > 1)
                {
                    throw new Exception($"Please mark only one public static method"
                                        + $" with {attribute.Name}. Marked methods: {string.Join(", ", methodsWithAttribute)}");
                }
                if (methodsWithAttribute.Count != 1)
                {
                    return null;
                }

                var methodWithAttribute = methodsWithAttribute.First();
                if (!expectedReturnType.IsAssignableFrom(methodWithAttribute.ReturnType))
                {
                    throw new Exception(
                        $"Method marked with {attribute.Name} should return {expectedReturnType.Name} or its subclass. Method {methodWithAttribute} does not conform");
                }
                if (expectedParameterType == null && methodWithAttribute.GetParameters().Any())
                {
                    throw new Exception(
                        $"Method marked with {attribute.Name} should not accept any parameters. Method {methodWithAttribute} does not conform");
                }
                if (expectedParameterType != null &&
                    !methodWithAttribute.GetParameters()
                        .Select(p => p.ParameterType)
                        .ToArray()
                        .SequenceEqual(new[] { expectedParameterType }))
                {
                    throw new Exception(
                        $"Method marked with {attribute.Name} should accept exactly one parameter of type {expectedParameterType.Name}. Method {methodWithAttribute} does not conform");
                }
                return methodWithAttribute;
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem in class {pageType}: ", ex);
            }
        }
    }
}