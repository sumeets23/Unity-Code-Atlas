using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class AllocationInUpdateRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-GC-001";
        public string DisplayName => "Allocation inside Update-family method";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                if (!IsUpdateFamily(method.MethodName))
                {
                    continue;
                }

                for (var i = method.StartLine - 1; i < method.EndLine && i < sourceFile.Lines.Length; i++)
                {
                    var line = sourceFile.Lines[i];
                    if (line.Contains("new List<") || line.Contains("new Dictionary<") || line.Contains("new HashSet<") || line.Contains("new ") && line.Contains("[]"))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Creates managed memory during a per-frame Unity callback.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Critical);
                    }
                }
            }
        }

        private static bool IsUpdateFamily(string methodName)
        {
            return methodName == "Update" || methodName == "FixedUpdate" || methodName == "LateUpdate";
        }
    }
}
