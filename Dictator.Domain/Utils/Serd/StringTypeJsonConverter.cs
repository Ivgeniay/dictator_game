using Dictator.Domain.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

namespace Dictator.Domain.Utils.Serd
{
    public class StringTypeJsonConverter : JsonConverter<StringType>
    {
        public override StringType ReadJson(
            JsonReader reader,
            Type objectType,
            StringType existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeName = jo["Type"]?.Value<string>();

            if (string.IsNullOrEmpty(typeName))
                throw new JsonSerializationException("Поле Type отсутствует или пустое.");

            var type = typeof(StringType).Assembly.GetType(typeName);

            if (type == null)
                throw new JsonSerializationException($"Тип не найден: {typeName}");

            if (!typeof(StringType).IsAssignableFrom(type))
                throw new JsonSerializationException($"Тип {typeName} не является наследником StringType.");

            return (StringType)jo.ToObject(type, serializer);
        }

        public override void WriteJson(JsonWriter writer, StringType value, JsonSerializer serializer)
        {
            var jo = JObject.FromObject(value, serializer);
            jo.WriteTo(writer);
        }
    }
}