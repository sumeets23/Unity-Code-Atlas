using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Health
{
    public sealed class ArchitectureHealthAnalyzer : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            var categories = new List<AnalysisCategoryScore>
            {
                BuildCouplingScore(report),
                BuildComplexityScore(report),
                BuildMaintainabilityScore(report),
                BuildPerformanceScore(report),
                BuildTestabilityScore(context, report),
                BuildSceneHealthScore(report)
            };

            var overall = categories.Count == 0 ? 100 : categories.Sum(category => category.Score) / categories.Count;
            report.SetArchitectureHealth(new ArchitectureHealthReport(overall, ToGrade(overall), categories));
        }

        private static AnalysisCategoryScore BuildCouplingScore(ScriptIntelligenceReport report)
        {
            var critical = report.CouplingMetrics.Count(metric => metric.Severity == AnalysisSeverity.Critical);
            var warning = report.CouplingMetrics.Count(metric => metric.Severity == AnalysisSeverity.Warning);
            var cycles = report.DependencyCycles.Count;
            var score = 100 - critical * 12 - warning * 6 - cycles * 10;
            return new AnalysisCategoryScore("Coupling", score, critical + " critical coupling nodes, " + cycles + " cycles");
        }

        private static AnalysisCategoryScore BuildComplexityScore(ScriptIntelligenceReport report)
        {
            var largeNodes = report.ScriptGraphNodes.Count(node => node.Methods.Count >= 12 || node.Fields.Count >= 10);
            var longMethodIssues = report.Issues.Count(issue => issue.RuleId == "SI-MAINT-001");
            var score = 100 - largeNodes * 7 - longMethodIssues * 5;
            return new AnalysisCategoryScore("Complexity", score, largeNodes + " large classes, " + longMethodIssues + " long methods");
        }

        private static AnalysisCategoryScore BuildMaintainabilityScore(ScriptIntelligenceReport report)
        {
            var criticalIssues = report.Issues.Count(issue => issue.Severity == AnalysisSeverity.Critical);
            var warningIssues = report.Issues.Count(issue => issue.Severity == AnalysisSeverity.Warning);
            var score = 100 - criticalIssues * 10 - warningIssues * 3;
            return new AnalysisCategoryScore("Maintainability", score, criticalIssues + " critical issues, " + warningIssues + " warnings");
        }

        private static AnalysisCategoryScore BuildPerformanceScore(ScriptIntelligenceReport report)
        {
            var critical = report.PerformanceFindings.Count(finding => finding.Severity == AnalysisSeverity.Critical);
            var warnings = report.PerformanceFindings.Count(finding => finding.Severity == AnalysisSeverity.Warning);
            var score = 100 - critical * 12 - warnings * 5;
            return new AnalysisCategoryScore("Performance", score, critical + " critical risks, " + warnings + " warnings");
        }

        private static AnalysisCategoryScore BuildTestabilityScore(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            var singletonRelationships = report.ScriptRelationshipEdges.Count(edge => edge.RelationshipType == ScriptRelationshipType.SingletonUsage);
            var publicMutableFields = context.Fields.Count(field => field.Visibility == "public" && !field.FieldType.Contains("readonly"));
            var score = 100 - singletonRelationships * 8 - publicMutableFields * 2;
            return new AnalysisCategoryScore("Testability", score, singletonRelationships + " singleton usages, " + publicMutableFields + " public fields");
        }

        private static AnalysisCategoryScore BuildSceneHealthScore(ScriptIntelligenceReport report)
        {
            return new AnalysisCategoryScore("Scene Health", report.SceneHealth.Score, report.SceneHealth.Findings.Count + " scene findings");
        }

        private static ArchitectureScoreGrade ToGrade(int score)
        {
            if (score >= 95)
            {
                return ArchitectureScoreGrade.APlus;
            }

            if (score >= 85)
            {
                return ArchitectureScoreGrade.A;
            }

            if (score >= 72)
            {
                return ArchitectureScoreGrade.B;
            }

            if (score >= 60)
            {
                return ArchitectureScoreGrade.C;
            }

            return ArchitectureScoreGrade.D;
        }
    }
}
