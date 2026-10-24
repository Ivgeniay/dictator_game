using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Errors;
using Dictator.Engine.Core.Primitives;
using Dictator.Engine.Core.Registry;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dictator.Engine.Core.Serialization
{
    public sealed class SerializationError : LawError
    {
        public const string InvalidJson = "json.invalid_json";
        public const string InvalidFormat = "json.invalid_format";
        public const string UnsupportedFormatVersion = "json.unsupported_format_version";

        public SerializationError(string code, string message, string jsonPath)
            : base(code, message)
        {
            JsonPath = jsonPath;
        }

        public string JsonPath { get; }

        public override string ToString() => Code + " [" + JsonPath + "]: " + Message;
    }

    public sealed class LawJsonFormatException : Exception
    {
        public LawJsonFormatException(string jsonPath, string message)
            : base(message)
        {
            JsonPath = jsonPath;
        }

        public string JsonPath { get; }
    }

    public readonly struct SerializationResult<T>
    {
        private SerializationResult(T value, IReadOnlyList<SerializationError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public T Value { get; }

        public IReadOnlyList<SerializationError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;

        public static SerializationResult<T> Success(T value) => new SerializationResult<T>(value, Array.Empty<SerializationError>());

        public static SerializationResult<T> Fail(SerializationError error) => new SerializationResult<T>(default!, new[] { error });
    }

    public interface IDefinitionJsonCodec
    {
        string Kind { get; }

        bool CanWrite(Definition definition);

        void Write(Definition definition, JObject target, LawJsonSerializer serializer);

        Definition Read(JObject source, LawJsonSerializer serializer);
    }

    public sealed class LawJsonSerializer
    {
        public const int FormatVersion = 1;

        private readonly Dictionary<string, ScalarType> _scalars = new Dictionary<string, ScalarType>(StringComparer.Ordinal);
        private readonly Dictionary<string, IDefinitionJsonCodec> _codecs = new Dictionary<string, IDefinitionJsonCodec>(StringComparer.Ordinal);
        private readonly NodeWriter _nodeWriter;

        public LawJsonSerializer()
        {
            _nodeWriter = new NodeWriter(this);
            var builtins = new[]
            {
                LawTypes.Unit, LawTypes.Boolean, LawTypes.Integer, LawTypes.Decimal, LawTypes.String, LawTypes.Instant,
                LawTypes.Duration, LawTypes.TimeOfDay, LawTypes.TimeOfDayInterval, LawTypes.Distance, LawTypes.Position,
                LawTypes.Snapshot, LawTypes.Outcome, LawTypes.Verdict, LawTypes.Sanction
            };

            foreach (var scalar in builtins)
            {
                RegisterScalarType(scalar);
            }
        }

        public bool RegisterScalarType(ScalarType type)
        {
            if (_scalars.ContainsKey(type.Id) || type.Id == LawTypes.Never.ToString())
            {
                return false;
            }

            _scalars.Add(type.Id, type);
            return true;
        }

        public bool RegisterDefinitionCodec(IDefinitionJsonCodec codec)
        {
            if (_codecs.ContainsKey(codec.Kind))
            {
                return false;
            }

            _codecs.Add(codec.Kind, codec);
            return true;
        }

        public SerializationResult<string> WriteRegistry(DefinitionRegistry registry)
        {
            if (registry.RootActId == null || !registry.TryGetAct(registry.RootActId, out var root))
            {
                return WriteActs(Enumerable.Empty<Act>());
            }

            return WriteActs(new[] { root }.Concat(registry.Descendants(root.Id)));
        }

        public SerializationResult<string> WriteActs(IEnumerable<Act> acts)
        {
            try
            {
                var document = new JObject
                {
                    ["format"] = FormatVersion,
                    ["acts"] = new JArray(acts.Select(WriteAct))
                };

                return SerializationResult<string>.Success(document.ToString(Formatting.Indented));
            }
            catch (LawJsonFormatException exception)
            {
                return SerializationResult<string>.Fail(new SerializationError(SerializationError.InvalidFormat, exception.Message, exception.JsonPath));
            }
        }

        public SerializationResult<IReadOnlyList<Act>> ReadActs(string json)
        {
            JToken document;
            try
            {
                using var reader = new JsonTextReader(new StringReader(json))
                {
                    DateParseHandling = DateParseHandling.None,
                    FloatParseHandling = FloatParseHandling.Decimal
                };
                document = JToken.ReadFrom(reader);
            }
            catch (JsonReaderException exception)
            {
                return SerializationResult<IReadOnlyList<Act>>.Fail(new SerializationError(SerializationError.InvalidJson, exception.Message, exception.Path ?? string.Empty));
            }

            try
            {
                var root = AsObject(document);
                var version = ReadLong(root, "format");
                if (version != FormatVersion)
                {
                    return SerializationResult<IReadOnlyList<Act>>.Fail(new SerializationError(
                        SerializationError.UnsupportedFormatVersion,
                        $"Версия формата {version} не поддерживается, ожидается {FormatVersion}",
                        root["format"]!.Path));
                }

                IReadOnlyList<Act> acts = ReadArray(root, "acts").Select(ReadAct).ToArray();
                return SerializationResult<IReadOnlyList<Act>>.Success(acts);
            }
            catch (LawJsonFormatException exception)
            {
                return SerializationResult<IReadOnlyList<Act>>.Fail(new SerializationError(SerializationError.InvalidFormat, exception.Message, exception.JsonPath));
            }
        }

        public JObject WriteAct(Act act)
        {
            var result = new JObject
            {
                ["id"] = act.Id,
                ["name"] = act.Name
            };

            if (act.ParentId != null)
            {
                result["parent"] = act.ParentId;
            }

            result["definitions"] = new JArray(act.Definitions.Select(WriteDefinition));
            return result;
        }

        public Act ReadAct(JToken token)
        {
            var source = AsObject(token);
            return new Act(
                ReadString(source, "id"),
                ReadString(source, "name"),
                ReadOptionalString(source, "parent"),
                ReadArray(source, "definitions").Select(ReadDefinition).ToArray());
        }

        public JObject WriteDefinition(Definition definition)
        {
            var result = new JObject
            {
                ["kind"] = definition.Kind,
                ["id"] = definition.Id,
                ["name"] = definition.Name
            };

            if (_codecs.TryGetValue(definition.Kind, out var codec) && codec.CanWrite(definition))
            {
                codec.Write(definition, result, this);
                return result;
            }

            switch (definition)
            {
                case StatusDefinition status:
                    result["applicability"] = WriteApplicability(status.Applicability);
                    if (status.ParentId != null)
                    {
                        result["parent"] = status.ParentId;
                    }

                    break;
                case AliasDefinition alias:
                    result["value"] = WriteValue(alias.Value);
                    break;
                case SanctionDefinition sanction:
                    result["parameters"] = WriteParameters(sanction.Parameters);
                    break;
                case NormDefinition norm:
                    result["parameters"] = WriteParameters(norm.Parameters);
                    result["body"] = WriteNode(norm.Body);
                    break;
                case StateDefinition _:
                case OrganDefinition _:
                case CurrencyDefinition _:
                    break;
                default:
                    throw new LawJsonFormatException(string.Empty, $"Нет сериализатора для определения вида «{definition.Kind}»");
            }

            return result;
        }

        public Definition ReadDefinition(JToken token)
        {
            var source = AsObject(token);
            var kind = ReadString(source, "kind");

            if (_codecs.TryGetValue(kind, out var codec))
            {
                return codec.Read(source, this);
            }

            var id = ReadString(source, "id");
            var name = ReadString(source, "name");
            switch (kind)
            {
                case DefinitionKinds.Status:
                    return new StatusDefinition(id, name, ReadApplicability(Property(source, "applicability")), ReadOptionalString(source, "parent"));
                case DefinitionKinds.State:
                    return new StateDefinition(id, name);
                case DefinitionKinds.Alias:
                    return new AliasDefinition(id, name, ReadValue(Property(source, "value")));
                case DefinitionKinds.Organ:
                    return new OrganDefinition(id, name);
                case DefinitionKinds.Currency:
                    return new CurrencyDefinition(id, name);
                case DefinitionKinds.Sanction:
                    return new SanctionDefinition(id, name, ReadParameters(source));
                case DefinitionKinds.Norm:
                    return new NormDefinition(id, name, ReadParameters(source), ReadNode(Property(source, "body")));
                default:
                    throw Error(source["kind"]!, $"Неизвестный вид определения «{kind}»");
            }
        }

        public JToken WriteType(LawType type)
        {
            switch (type)
            {
                case NeverType _:
                    return type.ToString();
                case ScalarType scalar:
                    return scalar.Id;
                case ListType list:
                    return new JObject { ["list"] = WriteType(list.Element) };
                case RecordType record:
                    return new JObject
                    {
                        ["record"] = new JArray(record.Fields.Select(f => new JObject { ["name"] = f.Name, ["type"] = WriteType(f.Type) }))
                    };
                case MoneyType money:
                    return new JObject { ["money"] = money.CurrencyId };
                case DefinitionReferenceType reference:
                    return new JObject { ["ref"] = reference.DefinitionKind };
                default:
                    throw new LawJsonFormatException(string.Empty, $"Нет сериализатора для типа {type}");
            }
        }

        public LawType ReadType(JToken token)
        {
            if (token.Type == JTokenType.String)
            {
                var id = token.Value<string>()!;
                if (id == LawTypes.Never.ToString())
                {
                    return LawTypes.Never;
                }

                if (_scalars.TryGetValue(id, out var scalar))
                {
                    return scalar;
                }

                throw Error(token, $"Неизвестный тип «{id}»");
            }

            var source = AsObject(token);
            if (source.ContainsKey("list"))
            {
                return LawTypes.ListOf(ReadType(source["list"]!));
            }

            if (source.ContainsKey("record"))
            {
                return new RecordType(ReadArray(source, "record").Select(f =>
                {
                    var field = AsObject(f);
                    return new RecordField(ReadString(field, "name"), ReadType(Property(field, "type")));
                }));
            }

            if (source.ContainsKey("money"))
            {
                return LawTypes.MoneyIn(ReadString(source, "money"));
            }

            if (source.ContainsKey("ref"))
            {
                return LawTypes.ReferenceTo(ReadString(source, "ref"));
            }

            throw Error(token, "Неизвестная запись типа");
        }

        public JToken WriteValue(LawValue value)
        {
            switch (value)
            {
                case UnitValue _:
                    return new JObject { ["kind"] = "unit" };
                case BooleanValue boolean:
                    return new JObject { ["kind"] = "boolean", ["value"] = boolean.Value };
                case IntegerValue integer:
                    return new JObject { ["kind"] = "integer", ["value"] = integer.Value };
                case DecimalValue number:
                    return new JObject { ["kind"] = "decimal", ["value"] = Text(number.Value) };
                case StringValue text:
                    return new JObject { ["kind"] = "string", ["value"] = text.Value };
                case InstantValue instant:
                    return new JObject { ["kind"] = "instant", ["seconds"] = Text(instant.Seconds) };
                case DurationValue duration:
                    return new JObject { ["kind"] = "duration", ["seconds"] = Text(duration.Seconds) };
                case TimeOfDayValue time:
                    return new JObject { ["kind"] = "time_of_day", ["seconds"] = Text(time.Seconds) };
                case TimeOfDayIntervalValue interval:
                    return new JObject { ["kind"] = "time_of_day_interval", ["start"] = Text(interval.Start.Seconds), ["end"] = Text(interval.End.Seconds) };
                case DistanceValue distance:
                    return new JObject { ["kind"] = "distance", ["meters"] = Text(distance.Value.Value) };
                case MoneyValue money:
                    return new JObject { ["kind"] = "money", ["amount"] = Text(money.Amount), ["currency"] = money.CurrencyId };
                case ListValue list:
                    return new JObject { ["kind"] = "list", ["element"] = WriteType(list.ElementType), ["items"] = new JArray(list.Items.Select(WriteValue)) };
                case RecordValue record:
                    var fields = new JObject();
                    foreach (var field in ((RecordType)record.Type).Fields)
                    {
                        fields[field.Name] = WriteValue(record.Fields[field.Name]);
                    }

                    return new JObject { ["kind"] = "record", ["fields"] = fields };
                case DefinitionReferenceValue reference:
                    return new JObject { ["kind"] = "ref", ["definitionKind"] = reference.DefinitionKind, ["id"] = reference.DefinitionId };
                case NotApplicableValue _:
                    return new JObject { ["kind"] = "not_applicable" };
                default:
                    throw new LawJsonFormatException(string.Empty, $"Значение {value} нельзя сохранить в законе");
            }
        }

        public LawValue ReadValue(JToken token)
        {
            var source = AsObject(token);
            var kind = ReadString(source, "kind");
            switch (kind)
            {
                case "unit":
                    return UnitValue.Instance;
                case "boolean":
                    return BooleanValue.Of(ReadBoolean(source, "value"));
                case "integer":
                    return new IntegerValue(ReadLong(source, "value"));
                case "decimal":
                    return new DecimalValue(ReadDecimal(source, "value"));
                case "string":
                    return new StringValue(ReadString(source, "value"));
                case "instant":
                    return new InstantValue(ReadDecimal(source, "seconds"));
                case "duration":
                    return new DurationValue(ReadDecimal(source, "seconds"));
                case "time_of_day":
                    return new TimeOfDayValue(ReadDecimal(source, "seconds"));
                case "time_of_day_interval":
                    return new TimeOfDayIntervalValue(new TimeOfDayValue(ReadDecimal(source, "start")), new TimeOfDayValue(ReadDecimal(source, "end")));
                case "distance":
                    return new DistanceValue(new Meter(ReadDecimal(source, "meters")));
                case "money":
                    return new MoneyValue(ReadDecimal(source, "amount"), ReadString(source, "currency"));
                case "list":
                    return new ListValue(ReadType(Property(source, "element")), ReadArray(source, "items").Select(ReadValue).ToArray());
                case "record":
                    var fields = AsObject(Property(source, "fields"));
                    return new RecordValue(fields.Properties().Select(p => new KeyValuePair<string, LawValue>(p.Name, ReadValue(p.Value))).ToArray());
                case "ref":
                    return new DefinitionReferenceValue(ReadString(source, "definitionKind"), ReadString(source, "id"));
                case "not_applicable":
                    return NotApplicableValue.Instance;
                default:
                    throw Error(source["kind"]!, $"Неизвестный вид значения «{kind}»");
            }
        }

        public JObject WriteNode(LawNode node) => node.Accept(_nodeWriter);

        public LawNode ReadNode(JToken token)
        {
            var source = AsObject(token);
            var kind = ReadString(source, "node");
            switch (kind)
            {
                case "literal":
                    return new LiteralNode(ReadValue(Property(source, "value")));
                case "ref":
                    return new DefinitionReferenceNode(ReadString(source, "id"));
                case "parameter":
                    return new ParameterNode(ReadString(source, "name"));
                case "local":
                    return new LocalNode(ReadString(source, "name"));
                case "declare":
                    return new DeclareNode(ReadString(source, "name"), ReadNode(Property(source, "value")));
                case "assign":
                    return new AssignNode(ReadString(source, "name"), ReadNode(Property(source, "value")));
                case "operation":
                    return new OperationNode(ReadString(source, "id"), ReadNodes(source, "operands"));
                case "and":
                    return new AndNode(ReadNode(Property(source, "left")), ReadNode(Property(source, "right")));
                case "or":
                    return new OrNode(ReadNode(Property(source, "left")), ReadNode(Property(source, "right")));
                case "not":
                    return new NotNode(ReadNode(Property(source, "operand")));
                case "builtin":
                    return new BuiltinCallNode(ReadString(source, "id"), ReadNodes(source, "arguments"));
                case "block":
                    return new BlockNode(ReadNodes(source, "expressions"));
                case "if":
                    return new IfNode(ReadNode(Property(source, "condition")), ReadNode(Property(source, "then")), ReadOptionalNode(source, "otherwise"));
                case "while":
                    return new WhileNode(ReadNode(Property(source, "condition")), ReadNode(Property(source, "body")));
                case "for_each":
                    return new ForEachNode(ReadNode(Property(source, "source")), ReadString(source, "variable"), ReadNode(Property(source, "body")));
                case "call":
                    return new CallNode(ReadString(source, "target"), ReadNodes(source, "arguments"));
                case "return":
                    return new ReturnNode(ReadNode(Property(source, "value")));
                case "break":
                    return new BreakNode();
                case "continue":
                    return new ContinueNode();
                case "field":
                    return new FieldNode(ReadNode(Property(source, "record")), ReadString(source, "field"));
                case "has_status":
                    return new HasStatusNode(ReadNode(Property(source, "subject")), ReadString(source, "status"));
                case "status_records":
                    return new StatusRecordsNode(ReadNode(Property(source, "subject")), ReadString(source, "status"));
                case "state":
                    return new StateNode(ReadNode(Property(source, "subject")), ReadString(source, "state"));
                case "count":
                    return new CountNode(ReadNode(Property(source, "source")), ReadString(source, "variable"), ReadOptionalNode(source, "predicate"));
                case "exists":
                    return new ExistsNode(ReadNode(Property(source, "source")), ReadString(source, "variable"), ReadNode(Property(source, "predicate")));
                case "all":
                    return new AllNode(ReadNode(Property(source, "source")), ReadString(source, "variable"), ReadNode(Property(source, "predicate")));
                case "filter":
                    return new FilterNode(ReadNode(Property(source, "source")), ReadString(source, "variable"), ReadNode(Property(source, "predicate")));
                case "sanction":
                    return new SanctionNode(ReadString(source, "id"), ReadNodes(source, "arguments"));
                case "verdict":
                    return new VerdictNode(ReadModality(source), ReadNodes(source, "sanctions"), ReadOptionalNode(source, "reason"));
                case "not_applicable":
                    return NotApplicableNode.Instance;
                default:
                    throw Error(source["node"]!, $"Неизвестный вид узла «{kind}»");
            }
        }

        public static JObject AsObject(JToken token) =>
            token as JObject ?? throw Error(token, "Ожидается объект");

        public static JToken Property(JObject source, string name) =>
            source.TryGetValue(name, StringComparison.Ordinal, out var value) && value.Type != JTokenType.Null
                ? value
                : throw Error(source, $"Нет обязательного поля «{name}»");

        public static string ReadString(JObject source, string name)
        {
            var token = Property(source, name);
            return token.Type == JTokenType.String ? token.Value<string>()! : throw Error(token, $"Поле «{name}» должно быть строкой");
        }

        public static string? ReadOptionalString(JObject source, string name) =>
            source.TryGetValue(name, StringComparison.Ordinal, out var value) && value.Type != JTokenType.Null ? ReadString(source, name) : null;

        public static bool ReadBoolean(JObject source, string name)
        {
            var token = Property(source, name);
            return token.Type == JTokenType.Boolean ? token.Value<bool>() : throw Error(token, $"Поле «{name}» должно быть логическим");
        }

        public static long ReadLong(JObject source, string name)
        {
            var token = Property(source, name);
            if (token.Type == JTokenType.Integer && token is JValue value && value.Value is long result)
            {
                return result;
            }

            throw Error(token, $"Поле «{name}» должно быть целым числом в пределах 64 бит");
        }

        public static decimal ReadDecimal(JObject source, string name)
        {
            var token = Property(source, name);
            switch (token.Type)
            {
                case JTokenType.String when decimal.TryParse(token.Value<string>(), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var parsed):
                    return parsed;
                case JTokenType.Integer when token is JValue value && value.Value is long integer:
                    return integer;
                case JTokenType.Float when token is JValue value && value.Value is decimal number:
                    return number;
                default:
                    throw Error(token, $"Поле «{name}» должно быть десятичным числом");
            }
        }

        public static JArray ReadArray(JObject source, string name) =>
            Property(source, name) as JArray ?? throw Error(source[name]!, $"Поле «{name}» должно быть массивом");

        public static LawJsonFormatException Error(JToken token, string message) => new LawJsonFormatException(token.Path, message);

        private JArray WriteParameters(IReadOnlyList<TypedParameter> parameters) =>
            new JArray(parameters.Select(p => new JObject { ["name"] = p.Name, ["type"] = WriteType(p.Type) }));

        private IEnumerable<TypedParameter> ReadParameters(JObject source) =>
            ReadArray(source, "parameters").Select(p =>
            {
                var parameter = AsObject(p);
                return new TypedParameter(ReadString(parameter, "name"), ReadType(Property(parameter, "type")));
            }).ToArray();

        private LawNode[] ReadNodes(JObject source, string name) => ReadArray(source, name).Select(ReadNode).ToArray();

        private LawNode? ReadOptionalNode(JObject source, string name) =>
            source.TryGetValue(name, StringComparison.Ordinal, out var value) && value.Type != JTokenType.Null ? ReadNode(value) : null;

        private static JArray WriteApplicability(SubjectKinds kinds)
        {
            var result = new JArray();
            if ((kinds & SubjectKinds.Person) != 0)
            {
                result.Add("person");
            }

            if ((kinds & SubjectKinds.Organization) != 0)
            {
                result.Add("organization");
            }

            return result;
        }

        private static SubjectKinds ReadApplicability(JToken token)
        {
            var kinds = SubjectKinds.None;
            foreach (var item in token as JArray ?? throw Error(token, "Область применимости должна быть массивом"))
            {
                switch (item.Type == JTokenType.String ? item.Value<string>() : null)
                {
                    case "person":
                        kinds |= SubjectKinds.Person;
                        break;
                    case "organization":
                        kinds |= SubjectKinds.Organization;
                        break;
                    default:
                        throw Error(item, "Ожидается «person» или «organization»");
                }
            }

            return kinds;
        }

        private static string WriteModality(Modality modality)
        {
            switch (modality)
            {
                case Modality.Permitted:
                    return "permitted";
                case Modality.Forbidden:
                    return "forbidden";
                default:
                    return "obligatory";
            }
        }

        private static Modality ReadModality(JObject source)
        {
            switch (ReadString(source, "modality"))
            {
                case "permitted":
                    return Modality.Permitted;
                case "forbidden":
                    return Modality.Forbidden;
                case "obligatory":
                    return Modality.Obligatory;
                default:
                    throw Error(source["modality"]!, "Модальность должна быть «permitted», «forbidden» или «obligatory»");
            }
        }

        private static string Text(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        private sealed class NodeWriter : ILawNodeVisitor<JObject>
        {
            private readonly LawJsonSerializer _serializer;

            public NodeWriter(LawJsonSerializer serializer)
            {
                _serializer = serializer;
            }

            public JObject VisitLiteral(LiteralNode node) => Node("literal", ("value", _serializer.WriteValue(node.Value)));

            public JObject VisitDefinitionReference(DefinitionReferenceNode node) => Node("ref", ("id", node.DefinitionId));

            public JObject VisitParameter(ParameterNode node) => Node("parameter", ("name", node.Name));

            public JObject VisitLocal(LocalNode node) => Node("local", ("name", node.Name));

            public JObject VisitDeclare(DeclareNode node) => Node("declare", ("name", node.Name), ("value", Write(node.Value)));

            public JObject VisitAssign(AssignNode node) => Node("assign", ("name", node.Name), ("value", Write(node.Value)));

            public JObject VisitOperation(OperationNode node) => Node("operation", ("id", node.OperationId), ("operands", WriteMany(node.Operands)));

            public JObject VisitAnd(AndNode node) => Node("and", ("left", Write(node.Left)), ("right", Write(node.Right)));

            public JObject VisitOr(OrNode node) => Node("or", ("left", Write(node.Left)), ("right", Write(node.Right)));

            public JObject VisitNot(NotNode node) => Node("not", ("operand", Write(node.Operand)));

            public JObject VisitBuiltinCall(BuiltinCallNode node) => Node("builtin", ("id", node.FunctionId), ("arguments", WriteMany(node.Arguments)));

            public JObject VisitBlock(BlockNode node) => Node("block", ("expressions", WriteMany(node.Expressions)));

            public JObject VisitIf(IfNode node) =>
                Node("if", ("condition", Write(node.Condition)), ("then", Write(node.Then)), ("otherwise", WriteOptional(node.Otherwise)));

            public JObject VisitWhile(WhileNode node) => Node("while", ("condition", Write(node.Condition)), ("body", Write(node.Body)));

            public JObject VisitForEach(ForEachNode node) =>
                Node("for_each", ("source", Write(node.Source)), ("variable", node.Variable), ("body", Write(node.Body)));

            public JObject VisitCall(CallNode node) => Node("call", ("target", node.TargetId), ("arguments", WriteMany(node.Arguments)));

            public JObject VisitReturn(ReturnNode node) => Node("return", ("value", Write(node.Value)));

            public JObject VisitBreak(BreakNode node) => Node("break");

            public JObject VisitContinue(ContinueNode node) => Node("continue");

            public JObject VisitField(FieldNode node) => Node("field", ("record", Write(node.Record)), ("field", node.FieldName));

            public JObject VisitHasStatus(HasStatusNode node) => Node("has_status", ("subject", Write(node.Subject)), ("status", node.StatusId));

            public JObject VisitStatusRecords(StatusRecordsNode node) => Node("status_records", ("subject", Write(node.Subject)), ("status", node.StatusId));

            public JObject VisitState(StateNode node) => Node("state", ("subject", Write(node.Subject)), ("state", node.StateId));

            public JObject VisitCount(CountNode node) => Query("count", node);

            public JObject VisitExists(ExistsNode node) => Query("exists", node);

            public JObject VisitAll(AllNode node) => Query("all", node);

            public JObject VisitFilter(FilterNode node) => Query("filter", node);

            public JObject VisitSanction(SanctionNode node) => Node("sanction", ("id", node.SanctionId), ("arguments", WriteMany(node.Arguments)));

            public JObject VisitVerdict(VerdictNode node) =>
                Node("verdict", ("modality", WriteModality(node.Modality)), ("sanctions", WriteMany(node.Sanctions)), ("reason", WriteOptional(node.Reason)));

            public JObject VisitNotApplicable(NotApplicableNode node) => Node("not_applicable");

            private JObject Query(string kind, ListQueryNode node) =>
                Node(kind, ("source", Write(node.Source)), ("variable", node.Variable), ("predicate", WriteOptional(node.Predicate)));

            private JObject Write(LawNode node) => node.Accept(this);

            private JToken? WriteOptional(LawNode? node) => node?.Accept(this);

            private JArray WriteMany(IReadOnlyList<LawNode> nodes) => new JArray(nodes.Select(Write));

            private static JObject Node(string kind, params (string Name, JToken? Value)[] properties)
            {
                var result = new JObject { ["node"] = kind };
                foreach (var (name, value) in properties)
                {
                    if (value != null)
                    {
                        result[name] = value;
                    }
                }

                return result;
            }
        }
    }
}
