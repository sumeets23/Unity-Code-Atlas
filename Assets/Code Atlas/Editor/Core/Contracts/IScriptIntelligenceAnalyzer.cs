using ScriptIntelligence.Editor.Analysis;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Core.Contracts
{
    public interface IScriptIntelligenceAnalyzer
    {
        void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report);
    }
}
