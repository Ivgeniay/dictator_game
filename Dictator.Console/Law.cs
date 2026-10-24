using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Primitives;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;

namespace Dictator.Console;

static class Law
{
    public const string Rub = "currency.rub";

    public static LawNode NotApplicable => NotApplicableNode.Instance;

    public static LawNode Param(string name) => new ParameterNode(name);

    public static LawNode Local(string name) => new LocalNode(name);

    public static LawNode Ref(string definitionId) => new DefinitionReferenceNode(definitionId);

    public static LawNode Int(long value) => new LiteralNode(new IntegerValue(value));

    public static LawNode Text(string value) => new LiteralNode(new StringValue(value));

    public static LawNode Rubles(decimal amount) => new LiteralNode(new MoneyValue(amount, Rub));

    public static LawNode StatusRef(string statusId) => new LiteralNode(new DefinitionReferenceValue(DefinitionKinds.Status, statusId));

    public static LawNode Op(string operationId, params LawNode[] operands) => new OperationNode(operationId, operands);

    public static LawNode Fn(string functionId, params LawNode[] arguments) => new BuiltinCallNode(functionId, arguments);

    public static LawNode And(LawNode left, LawNode right) => new AndNode(left, right);

    public static LawNode Not(LawNode operand) => new NotNode(operand);

    public static LawNode If(LawNode condition, LawNode then, LawNode? otherwise = null) => new IfNode(condition, then, otherwise);

    public static LawNode HasStatus(LawNode subject, string statusId) => new HasStatusNode(subject, statusId);

    public static LawNode Records(LawNode subject, string statusId) => new StatusRecordsNode(subject, statusId);

    public static LawNode Field(LawNode record, string name) => new FieldNode(record, name);

    public static LawNode Exists(LawNode source, string variable, LawNode predicate) => new ExistsNode(source, variable, predicate);

    public static LawNode Count(LawNode source, string variable, LawNode predicate) => new CountNode(source, variable, predicate);

    public static LawNode Sanction(string sanctionId, params LawNode[] arguments) => new SanctionNode(sanctionId, arguments);

    public static LawNode Verdict(Modality modality, LawNode? reason, params LawNode[] sanctions) => new VerdictNode(modality, sanctions, reason);

    public static TypedParameter P(string name, LawType type) => new TypedParameter(name, type);

    public static TimeOfDayValue Clock(int hours, int minutes = 0) => TimeOfDayValue.FromClock(hours, minutes);

    public static InstantValue At(int day, int hours, int minutes = 0) =>
        new InstantValue(day * TimeOfDayValue.SecondsPerDay + hours * 3600m + minutes * 60m);

    public static StatusAssignment Assigned(string statusId, string assignedBy) => new StatusAssignment(statusId, assignedBy, At(0, 9));

    public static SnapshotValue Subject(string subjectId, params StatusAssignment[] assignments) =>
        new SnapshotValue(subjectId, assignments, Array.Empty<string>());

    public static ListValue Distances(IEnumerable<decimal> meters) =>
        new ListValue(LawTypes.Distance, meters.Select(m => (LawValue)new DistanceValue(new Meter(m))));
}
