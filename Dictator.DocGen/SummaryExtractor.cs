using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dictator.DocGen
{
    public class DocEntry
    {
        public string Kind { get; }
        public string Name { get; }
        public string Summary { get; }
        public int Order { get; }

        public DocEntry(string kind, string name, string summary, int order)
        {
            Kind = kind;
            Name = name;
            Summary = summary;
            Order = order;
        }
    }

    public class SummaryExtractor
    {
        public IReadOnlyList<DocEntry> Extract(string filePath)
        {
            var source = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(source);
            var root = tree.GetCompilationUnitRoot();

            var result = new List<DocEntry>();
            int order = 0;

            foreach (var node in root.DescendantNodes())
            {
                string kind = null;
                string name = null;

                switch (node)
                {
                    case ClassDeclarationSyntax c:
                        kind = "Класс";
                        name = BuildFullName(c);
                        break;
                    case InterfaceDeclarationSyntax i:
                        kind = "Интерфейс";
                        name = BuildFullName(i);
                        break;
                    case EnumDeclarationSyntax e:
                        kind = "Перечисление";
                        name = BuildFullName(e);
                        break;
                    case RecordDeclarationSyntax r:
                        kind = "Запись";
                        name = BuildFullName(r);
                        break;
                    case MethodDeclarationSyntax m when HasSummary(m):
                        kind = "Метод";
                        name = m.Identifier.Text;
                        break;
                    case PropertyDeclarationSyntax p when HasSummary(p):
                        kind = "Свойство";
                        name = p.Identifier.Text;
                        break;
                }

                if (kind == null) continue;

                var summary = ExtractSummary(node);
                if (string.IsNullOrWhiteSpace(summary)) continue;

                result.Add(new DocEntry(kind, name, summary, order++));
            }

            return result;
        }

        private static string BuildFullName(BaseTypeDeclarationSyntax node)
        {
            var parts = new List<string> { node.Identifier.Text };
            var parent = node.Parent;
            while (parent is BaseTypeDeclarationSyntax parentType)
            {
                parts.Insert(0, parentType.Identifier.Text);
                parent = parentType.Parent;
            }
            return string.Join(".", parts);
        }

        private static bool HasSummary(SyntaxNode node)
        {
            return !string.IsNullOrWhiteSpace(ExtractSummary(node));
        }

        private static string ExtractSummary(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                         || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                .ToList();

            if (!trivia.Any()) return null;

            var xml = trivia.Last().GetStructure();
            if (xml == null) return null;

            var summaryElement = xml.ChildNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "summary");

            if (summaryElement == null) return null;

            var sb = new StringBuilder();
            foreach (var content in summaryElement.Content)
            {
                if (content is XmlTextSyntax xmlText)
                {
                    foreach (var token in xmlText.TextTokens)
                    {
                        var text = token.ValueText
                            .TrimStart('/', ' ')
                            .Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                            sb.Append(text + " ");
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}