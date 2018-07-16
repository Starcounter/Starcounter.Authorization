using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.TestModel
{
    public class ClaimTemplate : IClaimTemplate
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string PropertiesSerialized { get; set; }
    }
}