using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class DebugLogInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-004";
        public string DisplayName => "Debug logging in hot path";

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
                    if (line.Contains("Debug.Log") || line.Contains("Debug.LogWarning") || line.Contains("Debug.LogError"))
                    {
                        yield return new ScriptIssue(
                            RuleId,
                            DisplayName,
                            "Logging in frame callbacks can allocate, flood the console, and hide real runtime issues.",
                            sourceFile.AssetPath,
                            i + 1,
                            AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
