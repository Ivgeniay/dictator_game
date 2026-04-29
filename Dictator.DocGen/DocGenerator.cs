using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dictator.DocGen
{
    public class DocGenerator
    {
        private readonly string _docsPath;
        private readonly string _solutionPath;

        public DocGenerator(string docsPath, string solutionPath)
        {
            _docsPath = docsPath;
            _solutionPath = solutionPath;
        }

        public void Generate(string sourceFilePath, IReadOnlyList<DocEntry> entries)
        {
            var relativeMd = BuildRelativeMdPath(sourceFilePath);
            var outputPath = Path.Combine(_docsPath, relativeMd);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var title = Path.GetFileNameWithoutExtension(sourceFilePath);
            var parent = Path.GetFileName(Path.GetDirectoryName(outputPath));

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("layout: default");
            sb.AppendLine($"title: {title}");
            sb.AppendLine($"parent: {parent}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"# {title}");
            sb.AppendLine();
            sb.AppendLine($"`{BuildRelativeSourcePath(sourceFilePath)}`");
            sb.AppendLine();

            foreach (var entry in entries)
            {
                sb.AppendLine($"## {entry.Name}");
                sb.AppendLine();
                sb.AppendLine($"*{entry.Kind}*");
                sb.AppendLine();
                sb.AppendLine(entry.Summary);
                sb.AppendLine();
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private string BuildRelativeMdPath(string sourceFilePath)
        {
            var relative = GetRelativePath(_solutionPath, sourceFilePath);
            return Path.ChangeExtension(relative, ".md");
        }

        private string BuildRelativeSourcePath(string sourceFilePath)
        {
            return GetRelativePath(_solutionPath, sourceFilePath);
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(basePath)
                ? fullPath.Substring(basePath.Length)
                : fullPath;
        }
    }
}