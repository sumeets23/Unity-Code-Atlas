using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class GetComponentInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-001";
        public string DisplayName => "GetComponent in hot path";

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
                    if (sourceFile.Lines[i].Contains("GetComponent"))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Cache component references outside per-frame callbacks when possible.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
