using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class CollectionAllocationInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-GC-004";
        public string DisplayName => "Collection allocation in hot path";

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
                    if (line.Contains("new List<") || line.Contains("new Dictionary<") || line.Contains("new HashSet<") || line.Contains(".ToList(") || line.Contains(".ToArray("))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Collection allocation in a per-frame callback can create avoidable GC pressure.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
