using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Types
{
    public delegate OperationResult OperationEvaluator(IReadOnlyList<LawValue> operands);

    public abstract class OperationOverload
    {
        private readonly OperationEvaluator _evaluate;

        protected OperationOverload(int arity, OperationEvaluator evaluate)
        {
            Arity = arity;
            _evaluate = evaluate;
        }

        public int Arity { get; }

        public OperationResult Evaluate(IReadOnlyList<LawValue> operands) => _evaluate(operands);

        public bool TryResolve(IReadOnlyList<LawType> operands, out LawType result)
        {
            if (operands.Count == Arity)
            {
                var resolved = ResolveCore(operands);
                if (resolved != null)
                {
                    result = resolved;
                    return true;
                }
            }

            result = NeverType.Instance;
            return false;
        }

        protected abstract LawType? ResolveCore(IReadOnlyList<LawType> operands);
    }

    public sealed class FixedOverload : OperationOverload
    {
        private readonly LawType[] _parameters;

        public FixedOverload(LawType result, OperationEvaluator evaluate, params LawType[] parameters)
            : base(parameters.Length, evaluate)
        {
            Result = result;
            _parameters = parameters.ToArray();
        }

        public LawType Result { get; }

        public IReadOnlyList<LawType> Parameters => _parameters;

        public bool HasSameParameters(FixedOverload other) => _parameters.SequenceEqual(other._parameters);

        protected override LawType? ResolveCore(IReadOnlyList<LawType> operands)
        {
            for (var i = 0; i < _parameters.Length; i++)
            {
                if (!operands[i].IsAssignableTo(_parameters[i]))
                {
                    return null;
                }
            }

            return Result;
        }
    }

    public sealed class RuleOverload : OperationOverload
    {
        private readonly Func<IReadOnlyList<LawType>, LawType?> _rule;

        public RuleOverload(int arity, Func<IReadOnlyList<LawType>, LawType?> rule, OperationEvaluator evaluate)
            : base(arity, evaluate)
        {
            _rule = rule;
        }

        protected override LawType? ResolveCore(IReadOnlyList<LawType> operands) => _rule(operands);
    }

    public enum OperationResolutionStatus
    {
        Resolved,
        UnknownOperation,
        NoMatchingOverload,
        Ambiguous
    }

    public readonly struct OperationResolution
    {
        private OperationResolution(OperationResolutionStatus status, LawType resultType, OperationOverload? overload, IReadOnlyList<OperationOverload> candidates)
        {
            Status = status;
            ResultType = resultType;
            Overload = overload;
            Candidates = candidates;
        }

        public OperationResolutionStatus Status { get; }

        public LawType ResultType { get; }

        public OperationOverload? Overload { get; }

        public IReadOnlyList<OperationOverload> Candidates { get; }

        public static OperationResolution Resolved(LawType resultType, OperationOverload? overload) =>
            new OperationResolution(OperationResolutionStatus.Resolved, resultType, overload, Array.Empty<OperationOverload>());

        public static OperationResolution Failed(OperationResolutionStatus status, IReadOnlyList<OperationOverload> candidates) =>
            new OperationResolution(status, NeverType.Instance, null, candidates);
    }

    public sealed class OperationTable
    {
        private readonly Dictionary<string, List<OperationOverload>> _operations = new Dictionary<string, List<OperationOverload>>(StringComparer.Ordinal);

        public IEnumerable<string> OperationIds => _operations.Keys;

        public bool Contains(string operationId) => _operations.ContainsKey(operationId);

        public IReadOnlyList<OperationOverload> GetOverloads(string operationId) =>
            _operations.TryGetValue(operationId, out var overloads) ? (IReadOnlyList<OperationOverload>)overloads : Array.Empty<OperationOverload>();

        public bool Register(string operationId, OperationOverload overload)
        {
            if (!_operations.TryGetValue(operationId, out var overloads))
            {
                overloads = new List<OperationOverload>();
                _operations.Add(operationId, overloads);
            }

            if (overload is FixedOverload candidate
                && overloads.OfType<FixedOverload>().Any(existing => existing.HasSameParameters(candidate)))
            {
                return false;
            }

            overloads.Add(overload);
            return true;
        }

        public OperationResolution Resolve(string operationId, IReadOnlyList<LawType> operands)
        {
            if (!_operations.TryGetValue(operationId, out var overloads))
            {
                return OperationResolution.Failed(OperationResolutionStatus.UnknownOperation, Array.Empty<OperationOverload>());
            }

            if (operands.Any(operand => operand is NeverType))
            {
                return overloads.Any(overload => overload.Arity == operands.Count)
                    ? OperationResolution.Resolved(NeverType.Instance, null)
                    : OperationResolution.Failed(OperationResolutionStatus.NoMatchingOverload, overloads);
            }

            var matches = new List<OperationOverload>();
            var resultType = (LawType)NeverType.Instance;
            foreach (var overload in overloads)
            {
                if (overload.TryResolve(operands, out var result))
                {
                    matches.Add(overload);
                    resultType = result;
                }
            }

            if (matches.Count == 1)
            {
                return OperationResolution.Resolved(resultType, matches[0]);
            }

            return matches.Count == 0
                ? OperationResolution.Failed(OperationResolutionStatus.NoMatchingOverload, overloads)
                : OperationResolution.Failed(OperationResolutionStatus.Ambiguous, matches);
        }
    }

    public static class OperationIds
    {
        public const string Add = "core.add";
        public const string Subtract = "core.subtract";
        public const string Multiply = "core.multiply";
        public const string Divide = "core.divide";
        public const string Remainder = "core.remainder";
        public const string Negate = "core.negate";
        public const string Equal = "core.equal";
        public const string NotEqual = "core.not_equal";
        public const string Less = "core.less";
        public const string LessOrEqual = "core.less_or_equal";
        public const string Greater = "core.greater";
        public const string GreaterOrEqual = "core.greater_or_equal";
        public const string Contains = "core.contains";
        public const string Overlaps = "core.overlaps";
    }
}
