using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dictator.DocGen
{
    public class DocsCleaner
    {
        private const string IgnoreFileName = ".autodocignore";
        private readonly string _docsPath;
        private readonly IReadOnlyList<string> _ignoreList;

        public DocsCleaner(string docsPath, IReadOnlyList<string> ignoreList)
        {
            _docsPath = docsPath;
            _ignoreList = ignoreList;
        }

        public void Clean()
        {
            var allFiles = Directory.GetFiles(_docsPath, "*", SearchOption.AllDirectories);
            var allDirs  = Directory.GetDirectories(_docsPath, "*", SearchOption.AllDirectories);

            var filesToDelete  = new List<string>();
            var filesToKeep    = new List<string>();
            var dirsToDelete   = new List<string>();
            var dirsToKeep     = new List<string>();

            foreach (var file in allFiles)
            {
                var relative = GetRelative(file);
                if (IsProtected(relative))
                    filesToKeep.Add(relative);
                else
                    filesToDelete.Add(relative);
            }

            foreach (var dir in allDirs.OrderByDescending(d => d.Length))
            {
                var relative = GetRelative(dir);
                if (IsProtected(relative) || HasProtectedDescendant(dir))
                    dirsToKeep.Add(relative);
                else
                    dirsToDelete.Add(relative);
            }

            // Console.WriteLine();
            // Console.WriteLine("=== ФАЙЛЫ К УДАЛЕНИЮ ===");
            // filesToDelete.ForEach(f => Console.WriteLine($"  - {f}"));

            // Console.WriteLine();
            // Console.WriteLine("=== ФАЙЛЫ К СОХРАНЕНИЮ ===");
            // filesToKeep.ForEach(f => Console.WriteLine($"  + {f}"));

            // Console.WriteLine();
            // Console.WriteLine("=== ДИРЕКТОРИИ К РЕКУРСИВНОМУ УДАЛЕНИЮ ===");
            // dirsToDelete.ForEach(d => Console.WriteLine($"  - {d}"));

            // Console.WriteLine();
            // Console.WriteLine("=== ДИРЕКТОРИИ К СОХРАНЕНИЮ ===");
            // dirsToKeep.ForEach(d => Console.WriteLine($"  + {d}"));

            // Console.WriteLine();

            foreach (var file in filesToDelete)
                File.Delete(Path.Combine(_docsPath, file));
            
            foreach (var dir in dirsToDelete.OrderByDescending(d => d.Length))
            {
                var full = Path.Combine(_docsPath, dir);
                if (Directory.Exists(full))
                    Directory.Delete(full, recursive: true);
            }
        }

        private bool IsProtected(string relative)
        {
            var name = Path.GetFileName(relative);

            if (string.Equals(name, IgnoreFileName, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var pattern in _ignoreList)
            {
                if (pattern.StartsWith("/"))
                {
                    var normalized = pattern.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var rel = relative.TrimStart(Path.DirectorySeparatorChar);

                    if (rel.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (rel.StartsWith(normalized + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                else
                {
                    if (string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private bool HasProtectedDescendant(string dirPath)
        {
            foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                if (IsProtected(GetRelative(file)))
                    return true;
            }
            foreach (var dir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
            {
                if (IsProtected(GetRelative(dir)))
                    return true;
            }
            return false;
        }

        private string GetRelative(string fullPath)
        {
            var basePath = _docsPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(basePath)
                ? fullPath.Substring(basePath.Length)
                : fullPath;
        }
    }
}