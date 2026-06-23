using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class RaycastInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-005";
        public string DisplayName => "Raycast in hot path";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                if (!IsHotPath(method.MethodName))
                {
                    continue;
                }

                for (var i = method.StartLine - 1; i < method.EndLine && i < sourceFile.Lines.Length; i++)
                {
                    var line = sourceFile.Lines[i];
                    if (line.Contains("Physics.Raycast") || line.Contains("Physics2D.Raycast") || line.Contains("RaycastAll"))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Raycasts in per-frame callbacks should be throttled, cached, or moved to event-driven queries.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }

        private static bool IsHotPath(string methodName)
        {
            return methodName == "Update" || methodName == "FixedUpdate" || methodName == "LateUpdate";
        }
    }
}
