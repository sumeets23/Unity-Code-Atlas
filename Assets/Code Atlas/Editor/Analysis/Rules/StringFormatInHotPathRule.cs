using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class StringFormatInHotPathRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-GC-005";
        public string DisplayName => "String formatting in hot path";

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
                    if (line.Contains("string.Format") || line.Contains("$\"") || line.Contains(".ToString("))
                    {
                        yield return new ScriptIssue(RuleId, DisplayName, "String formatting in per-frame code can allocate; cache labels or use pooled builders.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Warning);
                    }
                }
            }
        }
    }
}
