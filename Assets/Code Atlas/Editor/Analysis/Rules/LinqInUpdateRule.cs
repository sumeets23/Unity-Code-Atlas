using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class LinqInUpdateRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-GC-002";
        public string DisplayName => "LINQ inside Update-family method";

        private static readonly string[] LinqCalls =
        {
            ".Where(", ".Select(", ".OrderBy(", ".ThenBy(", ".ToList(", ".ToArray(", ".First(", ".FirstOrDefault(", ".Any(", ".All(", ".Count("
        };

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                if (method.MethodName != "Update" && method.MethodName != "FixedUpdate" && method.MethodName != "LateUpdate")
                {
                    continue;
                }

                for (var i = method.StartLine - 1; i < method.EndLine && i < sourceFile.Lines.Length; i++)
                {
                    var line = sourceFile.Lines[i];
                    foreach (var call in LinqCalls)
                    {
                        if (line.Contains(call))
                        {
                            yield return new ScriptIssue(RuleId, DisplayName, "LINQ often allocates enumerators or collections in hot paths.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                            break;
                        }
                    }
                }
            }
        }
    }
}
