using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public sealed class LargeSerializedCollectionRule : IScriptAnalysisRule
    {
        public string RuleId => "SI-MEM-001";
        public string DisplayName => "Large serialized collection";

        public IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods)
        {
            for (var i = 0; i < sourceFile.Lines.Length; i++)
            {
                var line = sourceFile.Lines[i];
                var previousLine = i > 0 ? sourceFile.Lines[i - 1] : string.Empty;
                if ((previousLine.Contains("SerializeField") || line.Contains("public ")) && (line.Contains("List<") || line.Contains("[]")))
                {
                    yield return new ScriptIssue(RuleId, DisplayName, "Serialized collections can become large hidden memory and scene-load costs. Prefer ScriptableObject data assets for large datasets.", sourceFile.AssetPath, i + 1, AnalysisSeverity.Info);
                }
            }
        }
    }
}
