using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class EmptyUpdateRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-PERF-003";
        public string DisplayName => "Empty Unity update callback";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            foreach (var method in methods)
            {
                if (method.MethodName != "Update" && method.MethodName != "FixedUpdate" && method.MethodName != "LateUpdate")
                {
                    continue;
                }

                var body = ExtractBodyContent(method.Body);

                if (string.IsNullOrEmpty(body) || body.StartsWith("//"))
                {
                    yield return new ScriptIssue(
                        RuleId,
                        DisplayName,
                        "Remove empty Update-family callbacks. Unity still invokes them and they add avoidable native-to-managed overhead.",
                        sourceFile.AssetPath,
                        method.StartLine,
                        AnalysisSeverity.Warning);
                }
            }
        }

        private static string ExtractBodyContent(string methodText)
        {
            var start = methodText.IndexOf('{');
            var end = methodText.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return string.Empty;
            }

            return methodText.Substring(start + 1, end - start - 1)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();
        }
    }
}
