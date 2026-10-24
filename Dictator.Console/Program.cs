using Dictator.Engine;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Interpretation;
using Dictator.Engine.Core.Primitives;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;
using static Dictator.Console.Law;

namespace Dictator.Console;

class Program
{
    static void Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        var engine = new LawEngine();
        Header("Принятие законодательства");
        Report("Конституция", engine.AddAct(Constitution()));
        Report("КоАП", engine.AddAct(AdminCode()));

        CurfewCase(engine);
        AuthorityCase(engine);
        PartyCase(engine);
        GatheringCase(engine);
        PersistenceCase(engine);
    }

    static Act Constitution() => new("act.constitution", "Конституция", null, new Definition[]
    {
        new CurrencyDefinition(Rub, "Рубль"),
        new OrganDefinition("organ.minjust", "Министерство юстиции"),
        new OrganDefinition("organ.police", "Полиция"),
        new StatusDefinition("status.foreign_agent", "Иностранный агент", SubjectKinds.Any),
        new StatusDefinition("status.minjust_employee", "Сотрудник Минюста", SubjectKinds.Person),
        new StatusDefinition("status.police_officer", "Сотрудник полиции", SubjectKinds.Person),
        new StatusDefinition("status.legal_entity", "Юридическое лицо", SubjectKinds.Organization),
        new StatusDefinition("status.nco", "НКО", SubjectKinds.Organization, "status.legal_entity"),
        new StatusDefinition("status.party", "Политическая партия", SubjectKinds.Organization, "status.nco"),
        new SanctionDefinition("sanction.fine", "Штраф", new[] { P("amount", LawTypes.MoneyIn(Rub)) }),
        new SanctionDefinition("sanction.annulment", "Аннулирование статуса", new[] { P("status", LawTypes.ReferenceTo(DefinitionKinds.Status)) }),
        new SanctionDefinition("sanction.none", "Санкций не предусмотрено", Array.Empty<TypedParameter>())
    });

    static Act AdminCode() => new("act.admin_code", "КоАП", "act.constitution", new Definition[]
    {
        new AliasDefinition("alias.curfew", "Комендантский час", new TimeOfDayIntervalValue(Clock(22), Clock(8))),
        new AliasDefinition("alias.gathering_radius", "Радиус точки сбора", new DistanceValue(new Meter(20))),

        new NormDefinition("norm.curfew", "Иноагент на улице в комендантский час",
            new[] { P("subject", LawTypes.Snapshot), P("now", LawTypes.Instant) },
            If(And(HasStatus(Param("subject"), "status.foreign_agent"),
                    Op(OperationIds.Contains, Ref("alias.curfew"), Fn(FunctionIds.TimeOfDay, Param("now")))),
                Verdict(Modality.Forbidden, null, Sanction("sanction.fine", Rubles(5000))),
                NotApplicable)),

        new NormDefinition("norm.agent_authority", "Статус иноагента присваивает только Минюст",
            new[] { P("subject", LawTypes.Snapshot), P("issuer", LawTypes.Snapshot) },
            If(And(Exists(Records(Param("subject"), "status.foreign_agent"), "record",
                        Op(OperationIds.Equal, Field(Local("record"), "assigned_by"), Fn(FunctionIds.SubjectId, Param("issuer")))),
                    Not(HasStatus(Param("issuer"), "status.minjust_employee"))),
                Verdict(Modality.Forbidden, Text("Статус присвоен лицом без полномочий"),
                    Sanction("sanction.annulment", StatusRef("status.foreign_agent"))),
                NotApplicable)),

        new NormDefinition("norm.nco_report", "Отчётность НКО",
            new[] { P("subject", LawTypes.Snapshot) },
            If(HasStatus(Param("subject"), "status.nco"),
                Verdict(Modality.Obligatory, Text("НКО ежеквартально сдаёт отчёт в Минюст"), Sanction("sanction.fine", Rubles(100_000))),
                NotApplicable)),

        new NormDefinition("norm.gathering", "Собрание в точке сбора",
            new[] { P("distances", LawTypes.ListOf(LawTypes.Distance)) },
            If(Op(OperationIds.Greater,
                    Count(Param("distances"), "d", Op(OperationIds.LessOrEqual, Local("d"), Ref("alias.gathering_radius"))),
                    Int(9)),
                Verdict(Modality.Forbidden, null, Sanction("sanction.none")),
                NotApplicable))
    });

    static void CurfewCase(LawEngine engine)
    {
        Header("Кейс 1. Комендантский час для иноагентов");

        var requirements = engine.GetRequirements("norm.curfew")!;
        Say($"Наблюдатель должен передать: {string.Join(", ", requirements.Parameters)}; статусы: {string.Join(", ", requirements.Statuses)}");

        var ivanov = Subject("person.ivanov", Assigned("status.foreign_agent", "person.kozlova"));
        var petrov = Subject("person.petrov");

        Ask(engine, "Иванов (иноагент) в 23:30", "norm.curfew", new() { ["subject"] = ivanov, ["now"] = At(3, 23, 30) });
        Ask(engine, "Иванов в 12:00", "norm.curfew", new() { ["subject"] = ivanov, ["now"] = At(3, 12) });
        Ask(engine, "Петров (обычный гражданин) в 23:30", "norm.curfew", new() { ["subject"] = petrov, ["now"] = At(3, 23, 30) });
        Ask(engine, "Иванов в 21:00", "norm.curfew", new() { ["subject"] = ivanov, ["now"] = At(4, 21) });

        Report("Указ №1 с ошибками", engine.AddAct(new Act("act.decree_1", "Указ №1", "act.constitution", new Definition[]
        {
            new NormDefinition("norm.decree_1", "Сверхурочный комендантский час",
                new[] { P("subject", LawTypes.Snapshot), P("now", LawTypes.Instant) },
                If(Op(OperationIds.Contains, Ref("alias.curfew"), Int(23)),
                    Verdict(Modality.Forbidden, null)))
        })));

        var version = engine.Version;
        Report("Указ №2: комендантский час с 20:00 до 06:00",
            engine.ReplaceDefinition(new AliasDefinition("alias.curfew", "Комендантский час", new TimeOfDayIntervalValue(Clock(20), Clock(6)))));
        Say($"  версия законодательства: {version} → {engine.Version}");
        Ask(engine, "Иванов в 21:00 после указа", "norm.curfew", new() { ["subject"] = ivanov, ["now"] = At(4, 21) });

        Report("Указ №3: комендантский час как число",
            engine.ReplaceDefinition(new AliasDefinition("alias.curfew", "Комендантский час", new IntegerValue(22))));
    }

    static void AuthorityCase(LawEngine engine)
    {
        Header("Кейс 2. Кто имеет право присвоить статус иноагента");

        var sidorov = Subject("person.sidorov", Assigned("status.police_officer", "organ.police"));
        var kozlova = Subject("person.kozlova", Assigned("status.minjust_employee", "organ.minjust"));
        var bySidorov = Subject("person.smirnova", Assigned("status.foreign_agent", "person.sidorov"));
        var byKozlova = Subject("person.orlov", Assigned("status.foreign_agent", "person.kozlova"));
        var forgedSidorov = Subject("person.sidorov",
            Assigned("status.police_officer", "organ.police"),
            Assigned("status.minjust_employee", "organ.minjust"));

        Ask(engine, "Смирнова, статус выдал полицейский Сидоров", "norm.agent_authority", new() { ["subject"] = bySidorov, ["issuer"] = sidorov });
        Ask(engine, "Орлов, статус выдала Козлова из Минюста", "norm.agent_authority", new() { ["subject"] = byKozlova, ["issuer"] = kozlova });
        Ask(engine, "Смирнова, подкупленный наблюдатель подделал снимок Сидорова", "norm.agent_authority",
            new() { ["subject"] = bySidorov, ["issuer"] = forgedSidorov });
    }

    static void PartyCase(LawEngine engine)
    {
        Header("Кейс 3. Партия отвечает как НКО");

        var requirements = engine.GetRequirements("norm.nco_report")!;
        Say($"Статусы, которые нужны наблюдателю: {string.Join(", ", requirements.Statuses)}");

        Ask(engine, "Партия «Новый путь»", "norm.nco_report", new() { ["subject"] = Subject("org.new_way", Assigned("status.party", "organ.minjust")) });
        Ask(engine, "Коммерческая фирма", "norm.nco_report", new() { ["subject"] = Subject("org.firm", Assigned("status.legal_entity", "organ.minjust")) });

        Report("Поправка в КоАП: партиям-иноагентам запрещена деятельность", engine.AddDefinition("act.admin_code",
            new NormDefinition("norm.foreign_party", "Партия-иноагент",
                new[] { P("subject", LawTypes.Snapshot) },
                If(And(HasStatus(Param("subject"), "status.party"), HasStatus(Param("subject"), "status.foreign_agent")),
                    Verdict(Modality.Forbidden, Text("Партия не может быть иностранным агентом"),
                        Sanction("sanction.annulment", StatusRef("status.party"))),
                    NotApplicable))));

        Ask(engine, "Партия «Новый путь», признанная иноагентом", "norm.foreign_party", new()
        {
            ["subject"] = Subject("org.new_way", Assigned("status.party", "organ.minjust"), Assigned("status.foreign_agent", "person.kozlova"))
        });

        Report("Поправка в КоАП с повторным названием", engine.AddDefinition("act.admin_code",
            new NormDefinition("norm.nco_report_2", "Отчётность НКО", new[] { P("subject", LawTypes.Snapshot) }, NotApplicable)));
    }

    static void GatheringCase(LawEngine engine)
    {
        Header("Кейс 4. Митинг: цена сложного закона");

        var crowd = Distances(Enumerable.Range(0, 300).Select(i => (decimal)(i % 50)));
        Ask(engine, "300 человек, бюджет gas 5000", "norm.gathering", new() { ["distances"] = crowd }, 5000);
        Ask(engine, "300 человек, бюджет gas 100", "norm.gathering", new() { ["distances"] = crowd }, 100);
        Ask(engine, "Ошибка наблюдателя: передал число вместо списка", "norm.gathering", new() { ["distances"] = new IntegerValue(300) });
    }

    static void PersistenceCase(LawEngine engine)
    {
        Header("Кейс 5. Сохранение и загрузка законодательства");

        var json = engine.ExportJson();
        Say($"Сохранено: {json.Value.Length} символов JSON");

        var restored = new LawEngine();
        Report("Загрузка в новое ядро", restored.ImportJson(json.Value));

        var ivanov = Subject("person.ivanov", Assigned("status.foreign_agent", "person.kozlova"));
        Ask(restored, "Иванов в 21:00 (загруженное ядро)", "norm.curfew", new() { ["subject"] = ivanov, ["now"] = At(5, 21) });

        var broken = json.Value.Replace("\"node\": \"verdict\"", "\"node\": \"execution\"");
        Report("Загрузка испорченного файла", new LawEngine().ImportJson(broken));
    }

    static void Ask(LawEngine engine, string label, string normId, Dictionary<string, LawValue> arguments, long gasLimit = 1000) =>
        Say($"  {label}\n    → {Describe(engine, engine.Evaluate(normId, arguments, gasLimit))}");

    static string Describe(LawEngine engine, EvaluationOutcome outcome)
    {
        switch (outcome.Status)
        {
            case EvaluationStatus.Verdict:
                var verdict = outcome.Verdict!;
                var sanctions = string.Join(", ", verdict.Sanctions.Select(s => Format(engine, s)));
                var reason = verdict.Reason is StringValue text ? $" — {text.Value}" : string.Empty;
                return $"{ModalityName(verdict.Modality)}: {sanctions}{reason} [gas {outcome.GasUsed}]";
            case EvaluationStatus.NotApplicable:
                return $"Не применима [gas {outcome.GasUsed}]";
            case EvaluationStatus.Aborted:
                return outcome.AbortReason == AbortReason.GasExhausted
                    ? $"Прервана: закон слишком сложен, gas исчерпан [gas {outcome.GasUsed}]"
                    : "Прервана: слишком глубокая цепочка вызовов";
            case EvaluationStatus.CallError:
                return "Ошибка вызова: " + string.Join("; ", outcome.Errors.Select(e => e.Message));
            default:
                return "Ошибка вычисления: " + string.Join("; ", outcome.Errors.Select(e => e.Message));
        }
    }

    static string Format(LawEngine engine, LawValue value)
    {
        switch (value)
        {
            case SanctionValue sanction:
                var arguments = sanction.Arguments.Count == 0 ? string.Empty : "(" + string.Join(", ", sanction.Arguments.Select(a => Format(engine, a))) + ")";
                return NameOf(engine, sanction.SanctionId) + arguments;
            case MoneyValue money:
                return $"{money.Amount:0.##} {NameOf(engine, money.CurrencyId)}";
            case DefinitionReferenceValue reference:
                return $"«{NameOf(engine, reference.DefinitionId)}»";
            default:
                return value.ToString() ?? string.Empty;
        }
    }

    static string NameOf(LawEngine engine, string definitionId) =>
        engine.TryGetDefinition(definitionId, out var definition) ? definition.Name : definitionId;

    static string ModalityName(Modality modality) => modality switch
    {
        Dictator.Engine.Core.Values.Modality.Permitted => "Разрешено",
        Dictator.Engine.Core.Values.Modality.Forbidden => "Запрещено",
        _ => "Обязательно"
    };

    static void Report(string label, LawResult result)
    {
        Say($"{label}: {(result.IsSuccess ? "принят" : "отклонён")}");
        foreach (var error in result.Errors)
        {
            Say($"    ✗ {error}");
        }
    }

    static void Header(string title) => Say($"\n=== {title} ===");

    static void Say(string text) => System.Console.WriteLine(text);
}
