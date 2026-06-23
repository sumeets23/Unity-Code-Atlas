using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.AI.Local
{
    public sealed class LocalArchitectureAssistant : IArchitectureAssistant
    {
        public ArchitectureAssistantResult GenerateInsights(ArchitectureAssistantContext context)
        {
            var report = context.Report;
            var recommendations = new List<ArchitectureAssistantRecommendation>();
            var mostCoupled = report.CouplingMetrics.OrderByDescending(metric => metric.ReferenceCount).FirstOrDefault();

            if (mostCoupled != null && mostCoupled.ReferenceCount > 0)
            {
                recommendations.Add(new ArchitectureAssistantRecommendation(
                    "Why is this script heavily coupled?",
                    mostCoupled.TypeName + " has " + mostCoupled.FanIn + " incoming and " + mostCoupled.FanOut + " outgoing type relationships. Classification: " + mostCoupled.Classification + ".",
                    mostCoupled.Severity));
            }

            if (report.DependencyCycles.Count > 0)
            {
                recommendations.Add(new ArchitectureAssistantRecommendation(
                    "What are major risks?",
                    "The graph contains " + report.DependencyCycles.Count + " dependency cycles. These can make initialization order, testing, and refactoring harder.",
                    AnalysisSeverity.Warning));
            }

            var firstDebtRecommendation = report.TechnicalDebt.Recommendations.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstDebtRecommendation))
            {
                recommendations.Add(new ArchitectureAssistantRecommendation(
                    "What should be refactored first?",
                    firstDebtRecommendation,
                    AnalysisSeverity.Warning));
            }

            var performanceRisk = report.PerformanceFindings.FirstOrDefault(finding => finding.Severity == AnalysisSeverity.Critical) ?? report.PerformanceFindings.FirstOrDefault();
            if (performanceRisk != null)
            {
                recommendations.Add(new ArchitectureAssistantRecommendation(
                    "What performance issues exist?",
                    performanceRisk.Title + " on " + performanceRisk.PlatformProfile + ": " + performanceRisk.Recommendation,
                    performanceRisk.Severity));
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add(new ArchitectureAssistantRecommendation(
                    "What architecture improvements are recommended?",
                    "No high-priority architecture risks were detected. Continue keeping dependencies explicit, scene references intentional, and hot-path allocations low.",
                    AnalysisSeverity.Info));
            }

            return new ArchitectureAssistantResult(recommendations);
        }
    }
}
