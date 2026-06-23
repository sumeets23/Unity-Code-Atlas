using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Architecture
{
    public sealed class GraphExportSnapshotBuilder : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            report.GraphSnapshot.Clear();

            foreach (var node in report.ScriptGraphNodes.OrderBy(node => node.ClassName))
            {
                var coupling = report.CouplingMetrics.FirstOrDefault(metric => metric.TypeName == node.ClassName);
                var issues = report.Issues.Count(issue => issue.AssetPath == node.AssetPath);
                var complexity = node.Methods.Count * 2 + node.Fields.Count + (coupling == null ? 0 : coupling.ReferenceCount) + issues * 3;
                report.GraphSnapshot.Nodes.Add(new GraphExportNode(
                    node.ClassName,
                    node.ClassName,
                    node.NodeType,
                    coupling == null ? "Healthy" : coupling.Classification,
                    complexity,
                    coupling == null ? 0 : coupling.ReferenceCount,
                    issues,
                    node.AssetPath));
            }

            foreach (var group in report.ScriptRelationshipEdges.GroupBy(edge => edge.FromType + "|" + edge.ToType + "|" + edge.RelationshipType))
            {
                var first = group.First();
                var fromSeverity = report.CouplingMetrics.FirstOrDefault(metric => metric.TypeName == first.FromType);
                var toSeverity = report.CouplingMetrics.FirstOrDefault(metric => metric.TypeName == first.ToType);
                report.GraphSnapshot.Edges.Add(new GraphExportEdge(
                    first.FromType,
                    first.ToType,
                    first.RelationshipType,
                    group.Count(),
                    MaxSeverity(fromSeverity == null ? AnalysisSeverity.Info : fromSeverity.Severity, toSeverity == null ? AnalysisSeverity.Info : toSeverity.Severity)));
            }
        }

        private static AnalysisSeverity MaxSeverity(AnalysisSeverity left, AnalysisSeverity right)
        {
            return left > right ? left : right;
        }
    }
}
