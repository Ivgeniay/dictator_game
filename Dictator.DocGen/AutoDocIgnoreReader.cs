namespace Dictator.DocGen
{
    public static class AutoDocIgnoreReader
    {
        private const string IgnoreFileName = ".autodocignore";

        public static IReadOnlyList<string> Read(string docsPath)
        {
            var ignorePath = Path.Combine(docsPath, IgnoreFileName);
            var result = new List<string>();

            if (!File.Exists(ignorePath))
                return result;

            foreach (var line in File.ReadAllLines(ignorePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;
                result.Add(trimmed);
            }

            return result;
        }
    }
}