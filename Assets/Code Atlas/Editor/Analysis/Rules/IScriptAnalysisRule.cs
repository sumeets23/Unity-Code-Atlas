using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Rules
{
    public interface IScriptAnalysisRule
    {
        string RuleId { get; }
        string DisplayName { get; }
        IEnumerable<ScriptIssue> Analyze(SourceFile sourceFile, IReadOnlyList<ParsedMethod> methods);
    }
}
