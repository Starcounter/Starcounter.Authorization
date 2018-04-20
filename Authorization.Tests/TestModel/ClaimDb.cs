using System.Security.Claims;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class ClaimDb : IClaimDb
    {
        public string ClaimSerialized { get; set; }
    }
}