using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Architecture
{
    public sealed class ArchitectureInsightEngine : IScriptIntelligenceAnalyzer
    {
        private readonly IArchitectureAssistant assistant;

        public ArchitectureInsightEngine(IArchitectureAssistant assistant)
        {
            this.assistant = assistant;
        }

        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            report.SetArchitectureAssistantResult(assistant.GenerateInsights(new ArchitectureAssistantContext(report)));
        }
    }
}
