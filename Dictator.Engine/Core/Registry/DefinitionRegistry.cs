using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Errors;

namespace Dictator.Engine.Core.Registry
{
    public sealed class RegistryError : LawError
    {
        public const string DuplicateAct = "registry.duplicate_act";
        public const string UnknownAct = "registry.unknown_act";
        public const string UnknownParentAct = "registry.unknown_parent_act";
        public const string SecondRoot = "registry.second_root";
        public const string ActHasChildren = "registry.act_has_children";
        public const string DuplicateDefinition = "registry.duplicate_definition";
        public const string UnknownDefinition = "registry.unknown_definition";
        public const string DuplicateName = "registry.duplicate_name";
        public const string ShadowedName = "registry.shadowed_name";
        public const string UnresolvedReference = "registry.unresolved_reference";
        public const string InvisibleReference = "registry.invisible_reference";
        public const string KindChange = "registry.kind_change";
        public const string UsedSignatureChange = "registry.used_signature_change";
        public const string UsedDefinitionRemoval = "registry.used_definition_removal";

        public RegistryError(string code, string message, string? actId = null, string? definitionId = null)
            : base(code, message)
        {
            ActId = actId;
            DefinitionId = definitionId;
        }

        public string? ActId { get; }

        public string? DefinitionId { get; }
    }

    public readonly struct RegistryResult
    {
        private RegistryResult(IReadOnlyList<RegistryError> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<RegistryError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;

        public static RegistryResult Success() => new RegistryResult(Array.Empty<RegistryError>());

        public static RegistryResult Fail(IEnumerable<RegistryError> errors) => new RegistryResult(errors.ToArray());
    }

    public interface IChangePolicy
    {
        IEnumerable<RegistryError> CheckReplace(Definition current, Definition updated, IReadOnlyCollection<string> dependents);

        IEnumerable<RegistryError> CheckRemove(Definition current, IReadOnlyCollection<string> dependents);
    }

    public sealed class ForbidChangingUsedPolicy : IChangePolicy
    {
        public static readonly ForbidChangingUsedPolicy Instance = new ForbidChangingUsedPolicy();

        public IEnumerable<RegistryError> CheckReplace(Definition current, Definition updated, IReadOnlyCollection<string> dependents)
        {
            if (dependents.Count > 0 && !current.HasSameSignature(updated))
            {
                yield return new RegistryError(
                    RegistryError.UsedSignatureChange,
                    $"Нельзя изменить сигнатуру «{current.Name}»: на него ссылаются другие определения ({dependents.Count})",
                    definitionId: current.Id);
            }
        }

        public IEnumerable<RegistryError> CheckRemove(Definition current, IReadOnlyCollection<string> dependents)
        {
            if (dependents.Count > 0)
            {
                yield return new RegistryError(
                    RegistryError.UsedDefinitionRemoval,
                    $"Нельзя удалить «{current.Name}»: на него ссылаются другие определения ({dependents.Count})",
                    definitionId: current.Id);
            }
        }
    }

    public sealed class DefinitionRegistry
    {
        private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

        private readonly Dictionary<string, Act> _acts = new Dictionary<string, Act>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _childActs = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _owners = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, Definition> _definitions = new Dictionary<string, Definition>(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _dependents = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private readonly IChangePolicy _policy;

        public DefinitionRegistry(IChangePolicy? policy = null)
        {
            _policy = policy ?? ForbidChangingUsedPolicy.Instance;
        }

        public long Version { get; private set; }

        public string? RootActId { get; private set; }

        public IEnumerable<Act> Acts => _acts.Values;

        public bool TryGetAct(string actId, out Act act) => _acts.TryGetValue(actId, out act!);

        public bool TryGetDefinition(string definitionId, out Definition definition) => _definitions.TryGetValue(definitionId, out definition!);

        public string? GetOwnerActId(string definitionId) => _owners.TryGetValue(definitionId, out var actId) ? actId : null;

        public IReadOnlyList<string> GetChildActIds(string actId) =>
            _childActs.TryGetValue(actId, out var children) ? (IReadOnlyList<string>)children : Array.Empty<string>();

        public IReadOnlyCollection<string> GetDependents(string definitionId) =>
            _dependents.TryGetValue(definitionId, out var dependents) ? (IReadOnlyCollection<string>)dependents : Array.Empty<string>();

        public IEnumerable<Act> AncestorsAndSelf(string actId)
        {
            for (var current = actId; current != null && _acts.TryGetValue(current, out var act); current = act.ParentId)
            {
                yield return act;
            }
        }

        public IEnumerable<Act> Descendants(string actId)
        {
            foreach (var childId in GetChildActIds(actId))
            {
                yield return _acts[childId];
                foreach (var descendant in Descendants(childId))
                {
                    yield return descendant;
                }
            }
        }

        public IEnumerable<Definition> VisibleDefinitions(string actId) => AncestorsAndSelf(actId).SelectMany(act => act.Definitions);

        public bool IsVisible(string fromActId, string definitionId)
        {
            var owner = GetOwnerActId(definitionId);
            return owner != null && AncestorsAndSelf(fromActId).Any(act => string.Equals(act.Id, owner, StringComparison.Ordinal));
        }

        public IReadOnlyList<RegistryError> CheckAct(Act act)
        {
            var errors = new List<RegistryError>();

            if (_acts.ContainsKey(act.Id))
            {
                errors.Add(new RegistryError(RegistryError.DuplicateAct, $"Акт «{act.Name}» уже существует", act.Id));
            }

            if (act.ParentId == null)
            {
                if (RootActId != null)
                {
                    errors.Add(new RegistryError(RegistryError.SecondRoot, $"Акт «{act.Name}» не может быть вторым корневым актом", act.Id));
                }
            }
            else if (!_acts.ContainsKey(act.ParentId))
            {
                errors.Add(new RegistryError(RegistryError.UnknownParentAct, $"Вышестоящий акт для «{act.Name}» не найден", act.Id));
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            var namesAbove = new HashSet<string>(
                act.ParentId == null ? Enumerable.Empty<string>() : VisibleDefinitions(act.ParentId).Select(d => d.Name),
                NameComparer);
            var ownIds = new HashSet<string>(StringComparer.Ordinal);
            var ownNames = new HashSet<string>(NameComparer);

            foreach (var definition in act.Definitions)
            {
                if (_definitions.ContainsKey(definition.Id) || !ownIds.Add(definition.Id))
                {
                    errors.Add(new RegistryError(RegistryError.DuplicateDefinition, $"Определение с идентификатором «{definition.Id}» уже существует", act.Id, definition.Id));
                }

                if (namesAbove.Contains(definition.Name))
                {
                    errors.Add(new RegistryError(RegistryError.ShadowedName, $"Название «{definition.Name}» уже объявлено в вышестоящем акте", act.Id, definition.Id));
                }
                else if (!ownNames.Add(definition.Name))
                {
                    errors.Add(new RegistryError(RegistryError.DuplicateName, $"Название «{definition.Name}» объявлено в акте «{act.Name}» дважды", act.Id, definition.Id));
                }
            }

            foreach (var definition in act.Definitions)
            {
                errors.AddRange(CheckReferences(definition, act, ownIds));
            }

            return errors;
        }

        public RegistryResult AddAct(Act act)
        {
            var errors = CheckAct(act);
            if (errors.Count > 0)
            {
                return RegistryResult.Fail(errors);
            }

            _acts.Add(act.Id, act);
            if (act.ParentId == null)
            {
                RootActId = act.Id;
            }
            else
            {
                AddChild(act.ParentId, act.Id);
            }

            foreach (var definition in act.Definitions)
            {
                _definitions.Add(definition.Id, definition);
                _owners.Add(definition.Id, act.Id);
                LinkDependencies(definition);
            }

            Version++;
            return RegistryResult.Success();
        }

        public IReadOnlyList<RegistryError> CheckReplace(Definition updated)
        {
            if (!_definitions.TryGetValue(updated.Id, out var current))
            {
                return new[] { UnknownDefinitionError(updated.Id) };
            }

            var act = _acts[_owners[updated.Id]];
            var errors = new List<RegistryError>();

            if (!string.Equals(current.Kind, updated.Kind, StringComparison.Ordinal))
            {
                errors.Add(new RegistryError(RegistryError.KindChange, $"Нельзя изменить вид определения «{current.Name}»", act.Id, current.Id));
            }

            errors.AddRange(_policy.CheckReplace(current, updated, ExternalDependents(current.Id)));

            if (!NameComparer.Equals(current.Name, updated.Name))
            {
                errors.AddRange(CheckRenamed(updated, act));
            }

            errors.AddRange(CheckReferences(updated, act, new HashSet<string>(act.Definitions.Select(d => d.Id), StringComparer.Ordinal)));
            return errors;
        }

        public RegistryResult ReplaceDefinition(Definition updated)
        {
            var errors = CheckReplace(updated);
            if (errors.Count > 0)
            {
                return RegistryResult.Fail(errors);
            }

            var current = _definitions[updated.Id];
            var act = _acts[_owners[updated.Id]];
            UnlinkDependencies(current);
            _definitions[updated.Id] = updated;
            ReplaceAct(act, act.Definitions.Select(d => string.Equals(d.Id, updated.Id, StringComparison.Ordinal) ? updated : d));
            LinkDependencies(updated);

            Version++;
            return RegistryResult.Success();
        }

        public IReadOnlyList<RegistryError> CheckAddDefinition(string actId, Definition definition)
        {
            if (!_acts.TryGetValue(actId, out var act))
            {
                return new[] { new RegistryError(RegistryError.UnknownAct, $"Акт «{actId}» не найден", actId) };
            }

            var errors = new List<RegistryError>();
            if (_definitions.ContainsKey(definition.Id))
            {
                errors.Add(new RegistryError(RegistryError.DuplicateDefinition, $"Определение с идентификатором «{definition.Id}» уже существует", act.Id, definition.Id));
            }

            errors.AddRange(CheckRenamed(definition, act));

            var ownIds = new HashSet<string>(act.Definitions.Select(d => d.Id), StringComparer.Ordinal) { definition.Id };
            errors.AddRange(CheckReferences(definition, act, ownIds));
            return errors;
        }

        public RegistryResult AddDefinition(string actId, Definition definition)
        {
            var errors = CheckAddDefinition(actId, definition);
            if (errors.Count > 0)
            {
                return RegistryResult.Fail(errors);
            }

            var act = _acts[actId];
            _definitions.Add(definition.Id, definition);
            _owners.Add(definition.Id, act.Id);
            ReplaceAct(act, act.Definitions.Concat(new[] { definition }));
            LinkDependencies(definition);

            Version++;
            return RegistryResult.Success();
        }

        public RegistryResult RemoveDefinition(string definitionId)
        {
            if (!_definitions.TryGetValue(definitionId, out var current))
            {
                return RegistryResult.Fail(new[] { UnknownDefinitionError(definitionId) });
            }

            var errors = _policy.CheckRemove(current, ExternalDependents(definitionId)).ToList();
            if (errors.Count > 0)
            {
                return RegistryResult.Fail(errors);
            }

            var act = _acts[_owners[definitionId]];
            Unregister(current);
            ReplaceAct(act, act.Definitions.Where(d => !string.Equals(d.Id, definitionId, StringComparison.Ordinal)));

            Version++;
            return RegistryResult.Success();
        }

        public RegistryResult RemoveAct(string actId)
        {
            if (!_acts.TryGetValue(actId, out var act))
            {
                return RegistryResult.Fail(new[] { new RegistryError(RegistryError.UnknownAct, $"Акт «{actId}» не найден", actId) });
            }

            var errors = new List<RegistryError>();
            if (GetChildActIds(actId).Count > 0)
            {
                errors.Add(new RegistryError(RegistryError.ActHasChildren, $"Нельзя удалить акт «{act.Name}»: у него есть нижестоящие акты", actId));
            }

            var ownIds = new HashSet<string>(act.Definitions.Select(d => d.Id), StringComparer.Ordinal);
            foreach (var definition in act.Definitions)
            {
                var outside = GetDependents(definition.Id).Where(id => !ownIds.Contains(id)).ToArray();
                errors.AddRange(_policy.CheckRemove(definition, outside));
            }

            if (errors.Count > 0)
            {
                return RegistryResult.Fail(errors);
            }

            foreach (var definition in act.Definitions)
            {
                Unregister(definition);
            }

            _acts.Remove(actId);
            _childActs.Remove(actId);
            if (act.ParentId == null)
            {
                RootActId = null;
            }
            else
            {
                _childActs[act.ParentId].Remove(actId);
            }

            Version++;
            return RegistryResult.Success();
        }

        private IEnumerable<RegistryError> CheckReferences(Definition definition, Act act, HashSet<string> ownIds)
        {
            foreach (var dependency in definition.Dependencies)
            {
                if (ownIds.Contains(dependency))
                {
                    continue;
                }

                if (!_definitions.TryGetValue(dependency, out var target))
                {
                    yield return new RegistryError(
                        RegistryError.UnresolvedReference,
                        $"«{definition.Name}» ссылается на несуществующее определение «{dependency}»",
                        act.Id,
                        definition.Id);
                }
                else if (act.ParentId == null || !IsVisible(act.ParentId, dependency))
                {
                    yield return new RegistryError(
                        RegistryError.InvisibleReference,
                        $"Термин «{target.Name}» недоступен в акте «{act.Name}»",
                        act.Id,
                        definition.Id);
                }
            }
        }

        private IEnumerable<RegistryError> CheckRenamed(Definition updated, Act act)
        {
            var above = act.ParentId == null ? Enumerable.Empty<Definition>() : VisibleDefinitions(act.ParentId);
            if (above.Any(d => NameComparer.Equals(d.Name, updated.Name)))
            {
                yield return new RegistryError(RegistryError.ShadowedName, $"Название «{updated.Name}» уже объявлено в вышестоящем акте", act.Id, updated.Id);
            }

            if (act.Definitions.Any(d => !string.Equals(d.Id, updated.Id, StringComparison.Ordinal) && NameComparer.Equals(d.Name, updated.Name)))
            {
                yield return new RegistryError(RegistryError.DuplicateName, $"Название «{updated.Name}» уже объявлено в акте «{act.Name}»", act.Id, updated.Id);
            }

            foreach (var descendant in Descendants(act.Id))
            {
                if (descendant.Definitions.Any(d => NameComparer.Equals(d.Name, updated.Name)))
                {
                    yield return new RegistryError(
                        RegistryError.ShadowedName,
                        $"Название «{updated.Name}» уже используется в нижестоящем акте «{descendant.Name}»",
                        act.Id,
                        updated.Id);
                }
            }
        }

        private IReadOnlyCollection<string> ExternalDependents(string definitionId) =>
            GetDependents(definitionId).Where(id => !string.Equals(id, definitionId, StringComparison.Ordinal)).ToArray();

        private void AddChild(string parentId, string childId)
        {
            if (!_childActs.TryGetValue(parentId, out var children))
            {
                children = new List<string>();
                _childActs.Add(parentId, children);
            }

            children.Add(childId);
        }

        private void ReplaceAct(Act act, IEnumerable<Definition> definitions) =>
            _acts[act.Id] = new Act(act.Id, act.Name, act.ParentId, definitions);

        private void LinkDependencies(Definition definition)
        {
            foreach (var dependency in definition.Dependencies)
            {
                if (!_dependents.TryGetValue(dependency, out var dependents))
                {
                    dependents = new HashSet<string>(StringComparer.Ordinal);
                    _dependents.Add(dependency, dependents);
                }

                dependents.Add(definition.Id);
            }
        }

        private void UnlinkDependencies(Definition definition)
        {
            foreach (var dependency in definition.Dependencies)
            {
                if (_dependents.TryGetValue(dependency, out var dependents))
                {
                    dependents.Remove(definition.Id);
                    if (dependents.Count == 0)
                    {
                        _dependents.Remove(dependency);
                    }
                }
            }
        }

        private void Unregister(Definition definition)
        {
            UnlinkDependencies(definition);
            _definitions.Remove(definition.Id);
            _owners.Remove(definition.Id);
            _dependents.Remove(definition.Id);
        }

        private static RegistryError UnknownDefinitionError(string definitionId) =>
            new RegistryError(RegistryError.UnknownDefinition, $"Определение «{definitionId}» не найдено", definitionId: definitionId);
    }
}
