
namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing
{
    public partial class UnsecuredPage : Json
    {
        static UnsecuredPage()
        {
            DefaultTemplate.Common.InstanceType = typeof(CommonPart);
        }
    }
}
