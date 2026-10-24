# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A real-time city-builder / dictator simulator where the player issues laws (constitution, codes, decrees) and citizens live by them. The implemented part is the **law core** ("Язык законов"): typed law language, AST, validator, gas-metered interpreter, definitions/acts, definition registry, JSON serialization, and a facade. City simulation, agents and graphics are out of scope.

The design spec is `Dictator.Engine/LawEngineDeclare.md` (Russian). Markers: **[Принято]** decided, **[По умолчанию]** proposed default, **[Открыто]** undecided — don't invent answers to open items, flag them. Several decisions were made during implementation and are not yet in the spec (see "Decisions beyond the spec").

## Working rules (spec §15.2)

- Communicate in Russian. Identifiers in C# English.
- No comments and no `<summary>` XML doc blocks in code.
- Do not write code until the author explicitly asks.
- One file (or one logical module) per generation.
- Review sent code for weak spots, redundant data, incorrect behavior. If information is missing, say so.

## Build / run

```sh
dotnet build dictator.slnx
dotnet run --project Dictator.Console/Dictator.Console.csproj
```

VS Code tasks in `.vscode/tasks.json` wrap the same commands. There is no test project; `Dictator.Console/Program.cs` runs end-to-end scenario cases (curfew, status authority, NCO chain, gas cost, JSON round-trip) and serves as the de facto regression check — run it after engine changes.

## Constraints

- `Dictator.Engine` targets **netstandard2.1** → C# 8: block-scoped namespaces, no records, no `init`, no implicit usings, no target-typed `new()`, no collection expressions. Nullable is enabled. Only dependency: Newtonsoft.Json 13.0.3.
- `Dictator.Console` targets net10.0 (modern C#). Its namespace `Dictator.Console` shadows `System.Console` — use `System.Console.WriteLine`.
- `decimal` only, never `float`/`double` (determinism). Quantities in SI; time in seconds (`decimal`).

## Architecture

Entry point for the world is `Dictator.Engine/LawEngine.cs` (`LawEngine`). It owns the registry, validator, interpreter and serializer and enforces the order **registry structural check → validator → registry commit** for `AddAct` / `AddDefinition` / `ReplaceDefinition`, so unvalidated laws cannot enter the registry. `ImportJson` is all-or-nothing (rolls back added acts). `GetRequirements(normId)` statically derives what the observer must collect (parameters, statuses incl. descendant statuses, states, transitively through `Call`).

Modules under `Dictator.Engine/Core/`, in dependency order:

- `Types/` — `LawType` hierarchy (`ScalarType` with optional single parent, `NeverType`, `ListType` covariant, `RecordType`, `MoneyType<currency>`, `DefinitionReferenceType<kind>`); `OperationTable` resolves overloads by operand types and holds evaluators; `CoreOperations` / `CoreFunctions` build default operator and builtin-function tables (same table class). Mods extend by registering types/overloads, never AST nodes.
- `Values/` — immutable `LawValue` per type; `LegalValues.cs` has snapshot, status assignment, sanction, verdict, not-applicable. `OperationResult`/`EvaluationFailure` carry division-by-zero/overflow as data.
- `Ast/` — fixed set of 30 sealed nodes with `ILawNodeVisitor<T>`. Each node exposes `Children` with roles (`condition`, `operands[1]`…); error paths and JSON field names use the same role names.
- `Definitions/` — `Definition` base (id, name, kind; dependencies are **computed** by `DependencyCollector`, not stored; `HasSameSignature` defines what "signature" means per kind). `Act` contains its definitions; there is no owner field on definitions.
- `Registry/` — `DefinitionRegistry`: act tree, top-down visibility, no shadowing (names case-insensitive), dependency graph, `Version` counter. Every mutation has a side-effect-free `Check*` counterpart. "Can't change/remove used definitions" is a replaceable `IChangePolicy`.
- `Validation/` — `ActValidator` validates an act against the registry (own act + ancestors) and collects **all** errors with node paths. Erroneous nodes get type `Never` to suppress cascades.
- `Interpretation/` — tree-walking `Interpreter` with `Completion` flags (no exceptions for control flow), gas via overridable `GasPrices`, max call depth 128. Alias values and status chains are read from the registry **at call time**; outcomes record `RegistryVersion`.
- `Serialization/` — `LawJsonSerializer` (format version 1). Decimals written as strings; date parsing disabled. Internally throws `LawJsonFormatException`, but public read/write methods return results. Mods extend via `RegisterScalarType` and `IDefinitionJsonCodec`.
- `Errors/LawError` — base for registry, validation, call, evaluation and serialization errors. Errors are data everywhere at public boundaries.

## Decisions beyond the spec

- `core.unit` type for statement-like nodes; `If` without `else` and incompatible branches yield `Unit`.
- Norm result type is `core.outcome`; `core.verdict` is its subtype; `NotApplicable` has type `Outcome`.
- Money type is parameterized by currency; currency is its own definition kind.
- `HasStatus` / `StatusRecords` / `State` take a definition id, not an expression (enables static requirements).
- Added `Sanction(id, args)` node; `Forbidden`/`Obligatory` verdicts require ≥1 sanction ("no sanction" is an explicit sanction definition).
- Runtime "evaluation error" outcome (division by zero, overflow) in addition to the spec's §8.2 results.
- Time-of-day intervals are half-open `[start, end)`; equal start/end means empty. A game day is 86 400 law-seconds; instant 0 is midnight.
- Integer/decimal never mix implicitly (`core.to_decimal`, `core.truncate`); integer ÷ integer → decimal.
- `Call` can only target norms and returns `Outcome` (spec open question 6).
- Status assignment records expose only `status`, `assigned_by`, `assigned_at`; expiry/attributes exist in the snapshot but are not reachable from the language (no optional type yet).
