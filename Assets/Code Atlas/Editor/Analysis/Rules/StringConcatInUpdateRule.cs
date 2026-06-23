using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class StringConcatInUpdateRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-GC-003";
        public string DisplayName => "String construction in hot path";

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
                    if ((line.Contains("\"") && line.Contains("+")) || line.Contains("string.Format(") || line.Contains("$\""))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "String creation during per-frame callbacks can create avoidable GC pressure.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
