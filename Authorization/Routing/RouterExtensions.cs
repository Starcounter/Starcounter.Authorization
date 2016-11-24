using System.Linq;
using System.Reflection;

namespace Starcounter.Authorization.Routing
{
    public static class RouterExtensions
    {
        public static void RegisterAllFromAssembly(this Router router, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(type => type.GetCustomAttribute<UrlAttribute>() != null))
            {
                router.HandleGet(type);
            }
        }

        public static void RegisterAllFromCurrentAssembly(this Router router)
        {
            RegisterAllFromAssembly(router, Assembly.GetCallingAssembly());
        }
    }
}