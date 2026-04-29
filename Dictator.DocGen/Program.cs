using System;
using System.Collections.Generic;
using System.IO;

namespace Dictator.DocGen
{
    class Program
    {
        static int Main(string[] args)
        {
            string solutionPath = null;
            var includeProjects = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-p":
                    case "--path":
                        if (i + 1 < args.Length)
                            solutionPath = args[++i];
                        break;
                    case "-i":
                    case "--include":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            includeProjects.Add(args[++i]);
                        break;
                }
            }

            if (string.IsNullOrEmpty(solutionPath))
            {
                Console.Error.WriteLine("Ошибка: укажите путь к solution через -p или --path");
                PrintUsage();
                return 1;
            }

            if (!Directory.Exists(solutionPath))
            {
                Console.Error.WriteLine($"Ошибка: директория не найдена: {solutionPath}");
                return 1;
            }

            if (includeProjects.Count == 0)
            {
                Console.Error.WriteLine("Ошибка: укажите хотя бы один проект через -i или --include");
                PrintUsage();
                return 1;
            }

            var docsPath = Path.Combine(solutionPath, "docs");
            Directory.CreateDirectory(docsPath);

            Console.WriteLine($"Solution:  {solutionPath}");
            Console.WriteLine($"Проекты:   {string.Join(", ", includeProjects)}");
            Console.WriteLine($"Docs:      {docsPath}");
            Console.WriteLine();

            var ignoreList = AutoDocIgnoreReader.Read(docsPath);
            Console.WriteLine($"Защищённых путей: {ignoreList.Count}");

            var cleaner = new DocsCleaner(docsPath, ignoreList);
            cleaner.Clean();
            Console.WriteLine("Старые файлы удалены.");

            var scanner = new SolutionScanner(solutionPath, includeProjects);
            var files = scanner.Scan();
            Console.WriteLine($"Найдено .cs файлов: {files.Count}");

            var extractor = new SummaryExtractor();
            var generator = new DocGenerator(docsPath, solutionPath);
            var indexGenerator = new IndexGenerator(docsPath, includeProjects);

            int generated = 0;
            foreach (var file in files)
            {
                var entries = extractor.Extract(file);
                if (entries.Count == 0) continue;
                generator.Generate(file, entries);
                generated++;
            }

            indexGenerator.GenerateAll();

            Console.WriteLine($"Сгенерировано .md файлов: {generated}");
            Console.WriteLine("Готово.");
            return 0;
        }

        static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Использование:");
            Console.WriteLine("  Dictator.DocGen -p <путь> -i <проект1> <проект2>");
            Console.WriteLine();
            Console.WriteLine("Пример:");
            Console.WriteLine("  Dictator.DocGen -p /home/user/Dictator -i Dictator.Domain Dictator.Application");
        }
    }
}