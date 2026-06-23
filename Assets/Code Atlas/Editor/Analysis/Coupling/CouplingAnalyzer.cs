using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Coupling
{
    public sealed class CouplingAnalyzer : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            report.CouplingMetrics.Clear();

            foreach (var node in report.ScriptGraphNodes.OrderBy(node => node.ClassName))
            {
                var fanIn = report.ScriptRelationshipEdges.Select(edge => edge.FromType).Distinct().Count(type => report.ScriptRelationshipEdges.Any(edge => edge.FromType == type && edge.ToType == node.ClassName));
                var fanOut = report.ScriptRelationshipEdges.Where(edge => edge.FromType == node.ClassName).Select(edge => edge.ToType).Distinct().Count();
                var dependencyCount = report.DependencyEdges.Count(edge => edge.FromType == node.ClassName);
                var referenceCount = report.ScriptRelationshipEdges.Count(edge => edge.FromType == node.ClassName || edge.ToType == node.ClassName);
                var severity = ClassifySeverity(fanIn, fanOut, referenceCount);
                var classification = ClassifyNode(fanIn, fanOut, referenceCount, node.Methods.Count, node.Fields.Count);

                report.CouplingMetrics.Add(new CouplingMetric(node.ClassName, fanIn, fanOut, dependencyCount, referenceCount, severity, classification));
            }
        }

        private static AnalysisSeverity ClassifySeverity(int fanIn, int fanOut, int referenceCount)
        {
            if (fanOut >= 10 || referenceCount >= 16 || fanIn >= 12)
            {
                return AnalysisSeverity.Critical;
            }

            if (fanOut >= 6 || referenceCount >= 9 || fanIn >= 8)
            {
                return AnalysisSeverity.Warning;
            }

            return AnalysisSeverity.Info;
        }

        private static string ClassifyNode(int fanIn, int fanOut, int referenceCount, int methodCount, int fieldCount)
        {
            if (referenceCount == 0)
            {
                return "Isolated";
            }

            if (fanIn >= 8 && fanOut >= 6 || methodCount >= 18 || fieldCount >= 14)
            {
                return "God Class Candidate";
            }

            if (fanOut >= 6)
            {
                return "Over-Coupled";
            }

            if (fanIn == 0 && fanOut == 0)
            {
                return "Unused";
            }

            return "Healthy";
        }
    }
}
