using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Core.Contracts
{
    public interface IArchitectureAssistant
    {
        ArchitectureAssistantResult GenerateInsights(ArchitectureAssistantContext context);
    }
}
