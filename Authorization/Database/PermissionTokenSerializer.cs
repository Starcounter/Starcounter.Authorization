using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Starcounter.Authorization.Database
{
    /// <summary>
    /// Used to serialize instances of <see cref="PermissionToken"/>. Do not use for other purposes!
    /// Its implementation can be unsuitable for general use.
    /// </summary>
    public class PermissionTokenSerializer
    {
        private class DatabaseObjectsConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.GetObjectID());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType.GetCustomAttribute<DatabaseAttribute>() != null;
            }

            public override bool CanRead => false;
        }

        private static readonly DatabaseObjectsConverter Converter = new DatabaseObjectsConverter();

        public string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.None, Converter);
        }
    }
}