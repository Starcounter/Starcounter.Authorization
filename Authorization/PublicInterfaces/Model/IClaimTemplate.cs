namespace Starcounter.Authorization
{
    public interface IClaimTemplate
    {
        string Type { get; set; }
        string Value { get; set; }
        string ValueType { get; set; }
        string Issuer { get; set; }
        string OriginalIssuer { get; set; }
        string PropertiesSerialized { get; set; }
    }
}