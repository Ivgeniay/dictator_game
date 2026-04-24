using System;
using Dictator.Domain.Laws;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dictator.Domain.Utils.Serd
{
    public class NodeJsonConverter : JsonConverter<Node>
    {
        public override Node ReadJson(JsonReader reader, Type objectType, Node existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var type = jo["Type"]?.Value<string>();

            switch (type)
            {
                case nameof(LogicOperatorNode):
                    return jo.ToObject<LogicOperatorNode>(serializer);
                case nameof(SubjectNode):
                    return jo.ToObject<SubjectNode>(serializer);
                case nameof(SubjectGroupNode):
                    return jo.ToObject<SubjectGroupNode>(serializer);
                case nameof(ActionNode):
                    return jo.ToObject<ActionNode>(serializer);
                case nameof(RestrictionNode):
                    return jo.ToObject<RestrictionNode>(serializer);
                case nameof(CircumstanceNode):
                    return jo.ToObject<CircumstanceNode>(serializer);
                case nameof(CircumstanceGroupNode):
                    return jo.ToObject<CircumstanceGroupNode>(serializer);
                default:
                    throw new JsonSerializationException($"Неизвестный тип ноды: {type}");
            }
        }

        // public override void WriteJson(JsonWriter writer, Node value, JsonSerializer serializer)
        // {
        //     serializer.Serialize(writer, value);
        // }

        public override void WriteJson(JsonWriter writer, Node value, JsonSerializer serializer)
        {
            var jo = JObject.FromObject(value, serializer);
            jo.WriteTo(writer);
        }
    }
}