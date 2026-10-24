using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Errors;
using Dictator.Engine.Core.Interpretation;
using Dictator.Engine.Core.Registry;
using Dictator.Engine.Core.Serialization;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Validation;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine
{
    public readonly struct LawResult
    {
        private LawResult(IReadOnlyList<LawError> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<LawError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;

        public static LawResult Success() => new LawResult(Array.Empty<LawError>());

        public static LawResult Fail(IEnumerable<LawError> errors) => new LawResult(errors.ToArray());
    }

    public sealed class NormRequirements
    {
        public NormRequirements(string normId, IReadOnlyList<TypedParameter> parameters, IReadOnlyCollection<string> statuses, IReadOnlyCollection<string> states)
        {
            NormId = normId;
            Parameters = parameters;
            Statuses = statuses;
            States = states;
        }

        public string NormId { get; }

        public IReadOnlyList<TypedParameter> Parameters { get; }

        public IReadOnlyCollection<string> Statuses { get; }

        public IReadOnlyCollection<string> States { get; }
    }

    public sealed class LawEngine
    {
        private static readonly HashSet<string> ReferenceCodes = new HashSet<string>(StringComparer.Ordinal)
        {
            RegistryError.UnresolvedReference,
            RegistryError.InvisibleReference
        };

        private readonly DefinitionRegistry _registry;
        private readonly ActValidator _validator;
        private readonly Interpreter _interpreter;

        public LawEngine(
            OperationTable? operations = null,
            OperationTable? functions = null,
            GasPrices? prices = null,
            IChangePolicy? changePolicy = null,
            int maxCallDepth = Interpreter.DefaultMaxCallDepth)
        {
            Operations = operations ?? CoreOperations.CreateDefault();
            Functions = functions ?? CoreFunctions.CreateDefault();
            _registry = new DefinitionRegistry(changePolicy);
            _validator = new ActValidator(_registry, Operations, Functions);
            _interpreter = new Interpreter(_registry, Operations, Functions, prices, maxCallDepth);
        }

        public OperationTable Operations { get; }

        public OperationTable Functions { get; }

        public LawJsonSerializer Serializer { get; } = new LawJsonSerializer();

        public long Version => _registry.Version;

        public string? RootActId => _registry.RootActId;

        public IEnumerable<Act> Acts => _registry.Acts;

        public bool TryGetAct(string actId, out Act act) => _registry.TryGetAct(actId, out act);

        public bool TryGetDefinition(string definitionId, out Definition definition) => _registry.TryGetDefinition(definitionId, out definition);

        public string? GetOwnerActId(string definitionId) => _registry.GetOwnerActId(definitionId);

        public IReadOnlyCollection<string> GetDependents(string definitionId) => _registry.GetDependents(definitionId);

        public bool IsVisible(string fromActId, string definitionId) => _registry.IsVisible(fromActId, definitionId);

        public LawResult AddAct(Act act)
        {
            var structural = _registry.CheckAct(act);
            if (structural.Any(e => !ReferenceCodes.Contains(e.Code)))
            {
                return LawResult.Fail(structural);
            }

            var validation = _validator.Validate(act);
            if (!validation.IsSuccess)
            {
                return LawResult.Fail(validation.Errors);
            }

            return FromRegistry(_registry.AddAct(act));
        }

        public LawResult AddDefinition(string actId, Definition definition)
        {
            var structural = _registry.CheckAddDefinition(actId, definition);
            if (structural.Any(e => !ReferenceCodes.Contains(e.Code)))
            {
                return LawResult.Fail(structural);
            }

            if (_registry.TryGetAct(actId, out var act))
            {
                var candidate = new Act(act.Id, act.Name, act.ParentId, act.Definitions.Concat(new[] { definition }));
                var validation = _validator.Validate(candidate);
                if (!validation.IsSuccess)
                {
                    return LawResult.Fail(validation.Errors);
                }
            }

            return FromRegistry(_registry.AddDefinition(actId, definition));
        }

        public LawResult ReplaceDefinition(Definition updated)
        {
            var structural = _registry.CheckReplace(updated);
            if (structural.Any(e => !ReferenceCodes.Contains(e.Code)))
            {
                return LawResult.Fail(structural);
            }

            var ownerId = _registry.GetOwnerActId(updated.Id);
            if (ownerId != null && _registry.TryGetAct(ownerId, out var owner))
            {
                var candidate = new Act(
                    owner.Id,
                    owner.Name,
                    owner.ParentId,
                    owner.Definitions.Select(d => string.Equals(d.Id, updated.Id, StringComparison.Ordinal) ? updated : d));

                var validation = _validator.Validate(candidate);
                if (!validation.IsSuccess)
                {
                    return LawResult.Fail(validation.Errors);
                }
            }

            return FromRegistry(_registry.ReplaceDefinition(updated));
        }

        public LawResult RemoveDefinition(string definitionId) => FromRegistry(_registry.RemoveDefinition(definitionId));

        public LawResult RemoveAct(string actId) => FromRegistry(_registry.RemoveAct(actId));

        public EvaluationOutcome Evaluate(string normId, IReadOnlyDictionary<string, LawValue> arguments, long gasLimit) =>
            _interpreter.Evaluate(normId, arguments, gasLimit);

        public NormRequirements? GetRequirements(string normId)
        {
            if (!_registry.TryGetDefinition(normId, out var definition) || !(definition is NormDefinition norm))
            {
                return null;
            }

            var statuses = new HashSet<string>(StringComparer.Ordinal);
            var states = new HashSet<string>(StringComparer.Ordinal);
            CollectObserved(norm, statuses, states, new HashSet<string>(StringComparer.Ordinal));

            var allStatuses = _registry.Acts.SelectMany(a => a.Definitions).OfType<StatusDefinition>().ToArray();
            var expanded = new HashSet<string>(statuses, StringComparer.Ordinal);
            foreach (var status in allStatuses)
            {
                if (statuses.Any(target => IsKindOf(status.Id, target)))
                {
                    expanded.Add(status.Id);
                }
            }

            return new NormRequirements(norm.Id, norm.Parameters, expanded, states);
        }

        public SerializationResult<string> ExportJson() => Serializer.WriteRegistry(_registry);

        public LawResult ImportJson(string json)
        {
            var read = Serializer.ReadActs(json);
            if (!read.IsSuccess)
            {
                return LawResult.Fail(read.Errors);
            }

            var added = new List<string>();
            foreach (var act in read.Value)
            {
                var result = AddAct(act);
                if (!result.IsSuccess)
                {
                    for (var i = added.Count - 1; i >= 0; i--)
                    {
                        _registry.RemoveAct(added[i]);
                    }

                    return result;
                }

                added.Add(act.Id);
            }

            return LawResult.Success();
        }

        private void CollectObserved(NormDefinition norm, HashSet<string> statuses, HashSet<string> states, HashSet<string> visited)
        {
            if (!visited.Add(norm.Id))
            {
                return;
            }

            foreach (var node in Flatten(norm.Body))
            {
                switch (node)
                {
                    case HasStatusNode hasStatus:
                        statuses.Add(hasStatus.StatusId);
                        break;
                    case StatusRecordsNode records:
                        statuses.Add(records.StatusId);
                        break;
                    case StateNode state:
                        states.Add(state.StateId);
                        break;
                    case CallNode call when _registry.TryGetDefinition(call.TargetId, out var target) && target is NormDefinition callee:
                        CollectObserved(callee, statuses, states, visited);
                        break;
                }
            }
        }

        private bool IsKindOf(string statusId, string targetId)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);
            for (string? current = statusId; current != null && visited.Add(current);)
            {
                if (string.Equals(current, targetId, StringComparison.Ordinal))
                {
                    return true;
                }

                current = _registry.TryGetDefinition(current, out var definition) && definition is StatusDefinition status ? status.ParentId : null;
            }

            return false;
        }

        private static IEnumerable<LawNode> Flatten(LawNode node) =>
            new[] { node }.Concat(node.Children.SelectMany(child => Flatten(child.Node)));

        private static LawResult FromRegistry(RegistryResult result) =>
            result.IsSuccess ? LawResult.Success() : LawResult.Fail(result.Errors);
    }
}
