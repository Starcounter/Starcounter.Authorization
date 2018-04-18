namespace Starcounter.Authorization.Model.Serialization
{
    public interface IStringSerializer<T>
    {
        T Deserialize(string serialized);
        string Serialize(T @object);
    }
}