using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Debt
{
    public sealed class TechnicalDebtAnalyzer : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            var longMethods = report.Issues.Count(issue => issue.RuleId == "SI-MAINT-001");
            var highCoupling = report.CouplingMetrics.Count(metric => metric.Severity != AnalysisSeverity.Info);
            var cycles = report.DependencyCycles.Count;
            var updateAbuse = report.UpdateMethods.Count;
            var gcRisks = report.Issues.Count(issue => issue.RuleId.StartsWith("SI-GC"));
            var sceneProblems = report.SceneHealth.Findings.Count;

            var categories = new List<AnalysisCategoryScore>
            {
                new AnalysisCategoryScore("Long Methods", DebtCategoryScore(longMethods, 4), longMethods + " long method findings"),
                new AnalysisCategoryScore("High Coupling", DebtCategoryScore(highCoupling, 5), highCoupling + " coupling risks"),
                new AnalysisCategoryScore("Cycles", DebtCategoryScore(cycles, 10), cycles + " dependency cycles"),
                new AnalysisCategoryScore("Update Abuse", DebtCategoryScore(updateAbuse, 2), updateAbuse + " update callbacks"),
                new AnalysisCategoryScore("GC Risks", DebtCategoryScore(gcRisks, 4), gcRisks + " allocation risks"),
                new AnalysisCategoryScore("Scene Wiring", DebtCategoryScore(sceneProblems, 8), sceneProblems + " scene issues")
            };

            var debtScore = 100 - categories.Sum(category => 100 - category.Score) / categories.Count;
            var recommendations = BuildRecommendations(longMethods, highCoupling, cycles, updateAbuse, gcRisks, sceneProblems);
            report.SetTechnicalDebt(new TechnicalDebtReport(debtScore, categories, recommendations));
        }

        private static int DebtCategoryScore(int findingCount, int weight)
        {
            return 100 - findingCount * weight;
        }

        private static IEnumerable<string> BuildRecommendations(int longMethods, int highCoupling, int cycles, int updateAbuse, int gcRisks, int sceneProblems)
        {
            if (highCoupling > 0)
            {
                yield return "Reduce direct script references around high fan-out nodes before adding new systems.";
            }

            if (cycles > 0)
            {
                yield return "Break dependency cycles with interfaces, events, or data-only ScriptableObjects.";
            }

            if (longMethods > 0)
            {
                yield return "Split long methods into named operations that can be tested independently.";
            }

            if (updateAbuse > 8)
            {
                yield return "Consolidate per-frame work and gate Update callbacks behind active state checks.";
            }

            if (gcRisks > 0)
            {
                yield return "Move allocations out of hot paths and cache reusable collections or string builders.";
            }

            if (sceneProblems > 0)
            {
                yield return "Review scene wiring findings before prefab or scene handoff.";
            }
        }
    }
}
