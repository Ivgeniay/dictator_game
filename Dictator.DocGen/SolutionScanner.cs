namespace Dictator.DocGen
{
    public class SolutionScanner
    {
        private readonly string _solutionPath;
        private readonly IReadOnlyList<string> _includeProjects;

        public SolutionScanner(string solutionPath, IReadOnlyList<string> includeProjects)
        {
            _solutionPath = solutionPath;
            _includeProjects = includeProjects;
        }

        public IReadOnlyList<string> Scan()
        {
            var result = new List<string>();

            foreach (var projectName in _includeProjects)
            {
                var projectPath = Path.Combine(_solutionPath, projectName);

                if (!Directory.Exists(projectPath))
                {
                    System.Console.Error.WriteLine($"Предупреждение: проект не найден: {projectPath}");
                    continue;
                }

                var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (IsExcluded(file)) continue;
                    result.Add(file);
                }
            }

            return result;
        }

        private static bool IsExcluded(string filePath)
        {
            var name = Path.GetFileName(filePath);
            return name == "AssemblyInfo.cs"
                || name.EndsWith(".designer.cs")
                || filePath.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                || filePath.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar);
        }
    }
}