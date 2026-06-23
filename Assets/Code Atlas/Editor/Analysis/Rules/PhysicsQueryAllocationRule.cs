using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class PhysicsQueryAllocationRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-006";
        public string DisplayName => "Allocating physics query";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                for (var i = method.StartLine - 1; i < method.EndLine && i < sourceFile.Lines.Length; i++)
                {
                    var line = sourceFile.Lines[i];
                    if (line.Contains("OverlapSphere(") || line.Contains("OverlapBox(") || line.Contains("RaycastAll(") || line.Contains("SphereCastAll("))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "Use NonAlloc physics APIs or reuse buffers for repeated physics queries.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
