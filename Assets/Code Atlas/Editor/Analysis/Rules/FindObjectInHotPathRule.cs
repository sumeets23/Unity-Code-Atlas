using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class FindObjectInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-002";
        public string DisplayName => "Scene-wide lookup in hot path";

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
                    if (line.Contains("FindObjectOfType") || line.Contains("FindFirstObjectByType") || line.Contains("GameObject.Find"))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Scene-wide lookups in Update scale poorly with scene size.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Critical);
                    }
                }
            }
        }
    }
}
