using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.PageLoader
{
    public interface ISecurePage
    {
        Json Init(params object[] args);
    }

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