using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.PageLoader
{
    public class PageLoader
    {
        static PageLoader()
        {
            //            RuntimeHelpers.RunClassConstructor(typeof(RolePersonGroup).TypeHandle);
        }

        public Json Load(ISecurePage page, params object[] args)
        {
            try
            {
                return page.Init(args);
            }
            catch (UnauthorizedException)
            {
                var errorPage = new Json { ["Html"] = "/People/viewmodels/Unauthorized.html" };
                return errorPage;
            }
        }
    }
}