using Dictator.Domain.Laws.Bill;

namespace Dictator.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRestrictionStricter();
            TestRestrictionSofter();
            TestSubjectAdded();
            TestSubjectRemovedWithAll();
            TestCircumstanceAdded();
            TestActionChanged();
            TestActionNotChanged();

            Console.WriteLine("\nВсе тесты завершены.");
            Console.ReadLine();
        }

        static void PrintResult(string testName, BillStatementDiffResult result)
        {
            Console.WriteLine($"\n=== {testName} ===");
            Console.WriteLine($"Общее направление: {result.OverallDirection}");
            Console.WriteLine($"TotalDelta: {result.TotalDelta}");
            Console.WriteLine($"Restriction: {result.RestrictionChange.Direction} (delta: {result.RestrictionChange.Delta})");
            Console.WriteLine($"Action: {result.ActionChange.Presence}");

            foreach (var sc in result.SubjectChanges)
                Console.WriteLine($"  Subject [{sc.Presence}] {sc.Direction}: prev={sc.Previous.Subject} cur={sc.Current.Subject}");

            foreach (var cc in result.CircumstanceChanges)
                Console.WriteLine($"  Circumstance [{cc.Presence}] {cc.Direction}: prev={cc.Previous.Circumstance} cur={cc.Current.Circumstance}");
        }

        static BillStatement MakeStatement(
            SubjectType subject,
            ActionType action,
            RestrictionType restriction,
            CircumstanceType circumstance = null)
        {
            var subjects = new List<BillNode> { new BillSubjectNode(subject) };
            var circumstances = circumstance != null
                ? new List<BillNode> { new BillCircumstanceNode(circumstance) }
                : null;

            return new BillStatement(
                new BillSubjectGroupNode(subjects),
                new BillActionNode(action),
                new BillRestrictionNode(restriction),
                circumstances != null ? new BillCircumstanceGroupNode(circumstances) : null
            );
        }

        static void TestRestrictionStricter()
        {
            var previous = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.PartialRestriction);
            var current  = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            PrintResult("Ужесточение ограничения", BillStatementDiff.Compare(previous, current));
        }

        static void TestRestrictionSofter()
        {
            var previous = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            var current  = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.PartialRestriction);
            PrintResult("Смягчение ограничения", BillStatementDiff.Compare(previous, current));
        }

        static void TestSubjectAdded()
        {
            var previous = MakeStatement(SubjectType.Media,    ActionType.SpeakOnInternet, RestrictionType.FullBan);
            var current  = MakeStatement(SubjectType.Officials, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            PrintResult("Добавился новый субъект (запрет)", BillStatementDiff.Compare(previous, current));
        }

        static void TestSubjectRemovedWithAll()
        {
            var previousSubjects = new List<BillNode> { new BillSubjectNode(SubjectType.All) };
            var currentSubjects  = new List<BillNode> { new BillSubjectNode(SubjectType.Citizens) };

            var previous = new BillStatement(
                new BillSubjectGroupNode(previousSubjects),
                new BillActionNode(ActionType.SpeakOnInternet),
                new BillRestrictionNode(RestrictionType.FullBan));

            var current = new BillStatement(
                new BillSubjectGroupNode(currentSubjects),
                new BillActionNode(ActionType.SpeakOnInternet),
                new BillRestrictionNode(RestrictionType.FullBan));

            PrintResult("All → конкретная группа (смягчение)", BillStatementDiff.Compare(previous, current));
        }

        static void TestCircumstanceAdded()
        {
            var previous = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            var current  = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan, CircumstanceType.InTransport);
            PrintResult("Добавилось обстоятельство (запрет → смягчение)", BillStatementDiff.Compare(previous, current));
        }

        static void TestActionChanged()
        {
            var previous = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet,  RestrictionType.FullBan);
            var current  = MakeStatement(SubjectType.Citizens, ActionType.AssemblePublicly, RestrictionType.FullBan);
            PrintResult("Изменилось действие", BillStatementDiff.Compare(previous, current));
        }

        static void TestActionNotChanged()
        {
            var previous = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            var current  = MakeStatement(SubjectType.Citizens, ActionType.SpeakOnInternet, RestrictionType.FullBan);
            PrintResult("Действие не изменилось", BillStatementDiff.Compare(previous, current));
        }
    }
}