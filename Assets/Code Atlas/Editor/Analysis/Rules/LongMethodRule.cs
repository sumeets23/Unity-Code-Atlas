using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class LongMethodRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-MAINT-001";
        public string DisplayName => "Long method";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                var lineCount = method.EndLine - method.StartLine + 1;
                if (lineCount < 45)
                {
                    continue;
                }

                var severity = lineCount >= 90 ? AnalysisSeverity.Critical : AnalysisSeverity.Warning;
                yield return new ScriptIssue(
                    RuleId,
                    DisplayName,
                    method.ClassName + "." + method.MethodName + " is " + lineCount + " lines. Split orchestration, validation, and side effects into smaller units.",
                    sourceFile.AssetPath,
                    method.StartLine,
                    severity);
            }
        }
    }
}
