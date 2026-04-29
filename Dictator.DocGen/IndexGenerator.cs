using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dictator.DocGen
{
    public class IndexGenerator
    {
        private readonly string _docsPath;
        private readonly IReadOnlyList<string> _rootProjects;

        public IndexGenerator(string docsPath, IReadOnlyList<string> rootProjects)
        {
            _docsPath = docsPath;
            _rootProjects = rootProjects;
        }

        public void GenerateAll()
        {
            var rootDirs = _rootProjects
                .Select(p => Path.Combine(_docsPath, p))
                .Where(Directory.Exists)
                .OrderBy(d => d)
                .ToList();

            GenerateRootIndex(rootDirs);

            foreach (var rootDir in rootDirs)
            {
                GenerateForDirectory(rootDir, "Документация");

                foreach (var dir in Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories)
                    .OrderBy(d => d))
                {
                    var parentName = Path.GetFileName(Path.GetDirectoryName(dir));
                    GenerateForDirectory(dir, parentName);
                }
            }
        }

        private void GenerateRootIndex(IReadOnlyList<string> rootDirs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("layout: default");
            sb.AppendLine("title: Документация");
            sb.AppendLine("nav_order: 1");
            sb.AppendLine("has_children: true");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("# Документация");
            sb.AppendLine();

            if (rootDirs.Any())
            {
                sb.AppendLine("## Разделы");
                sb.AppendLine();
                foreach (var dir in rootDirs)
                {
                    var name = Path.GetFileName(dir);
                    sb.AppendLine($"- [{name}]({name}/index.md)");
                }
            }

            File.WriteAllText(Path.Combine(_docsPath, "index.md"), sb.ToString(), Encoding.UTF8);
        }

        private void GenerateForDirectory(string dirPath, string parentName)
        {
            var mdFiles = Directory.GetFiles(dirPath, "*.md")
                .Where(f => Path.GetFileName(f) != "index.md")
                .OrderBy(f => f)
                .ToList();

            var subDirs = Directory.GetDirectories(dirPath)
                .OrderBy(d => d)
                .ToList();

            if (!mdFiles.Any() && !subDirs.Any()) return;

            var dirName = Path.GetFileName(dirPath);

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("layout: default");
            sb.AppendLine($"title: {dirName}");
            sb.AppendLine($"parent: {parentName}");
            sb.AppendLine("has_children: true");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"# {dirName}");
            sb.AppendLine();

            if (subDirs.Any())
            {
                sb.AppendLine("## Разделы");
                sb.AppendLine();
                foreach (var sub in subDirs)
                {
                    var subName = Path.GetFileName(sub);
                    sb.AppendLine($"- [{subName}]({subName}/index.md)");
                }
                sb.AppendLine();
            }

            if (mdFiles.Any())
            {
                sb.AppendLine("## Файлы");
                sb.AppendLine();
                foreach (var file in mdFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    sb.AppendLine($"- [{fileName}]({Path.GetFileName(file)})");
                }
            }

            File.WriteAllText(Path.Combine(dirPath, "index.md"), sb.ToString(), Encoding.UTF8);
        }
    }
}