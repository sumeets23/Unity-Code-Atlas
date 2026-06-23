namespace ScriptIntelligence.Editor.Models
{
    public sealed class ArchitectureAssistantContext
    {
        public ScriptIntelligenceReport Report { get; }

        public ArchitectureAssistantContext(ScriptIntelligenceReport report)
        {
            Report = report;
        }
    }
}
