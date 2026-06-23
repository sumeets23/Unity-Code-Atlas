using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Scene
{
    public sealed class SceneHealthScanner : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            var findings = new List<string>();
            var suggestions = new List<string>();
            var duplicateManagers = report.SceneScriptNodes
                .GroupBy(node => node.TypeName)
                .Where(group => IsManagerType(group.Key) && group.Count() > 1)
                .ToList();

            foreach (var group in duplicateManagers)
            {
                findings.Add("Duplicate manager type in scene: " + group.Key + " appears " + group.Count() + " times.");
                suggestions.Add("Keep one authoritative " + group.Key + " or convert it to a scene-scoped service.");
            }

            foreach (var issue in report.SceneWiringIssues.OrderByDescending(issue => issue.Severity).ThenBy(issue => issue.ObjectPath))
            {
                findings.Add(issue.IssueType + ": " + issue.Description + " (" + issue.ObjectPath + ")");
                if (!string.IsNullOrEmpty(issue.FixSuggestion) && !suggestions.Contains(issue.FixSuggestion))
                {
                    suggestions.Add(issue.FixSuggestion);
                }
            }

            var sceneCycles = report.SceneDependencyEdges
                .Where(edge => report.SceneDependencyEdges.Any(other => other.FromType == edge.ToType && other.ToType == edge.FromType))
                .Select(edge => edge.FromType + " <-> " + edge.ToType)
                .Distinct()
                .ToList();

            foreach (var cycle in sceneCycles)
            {
                findings.Add("Circular scene reference: " + cycle);
            }

            if (sceneCycles.Count > 0)
            {
                suggestions.Add("Replace bidirectional scene references with an event, mediator, or explicit owner relationship.");
            }

            if (report.SceneScriptNodes.Count > 0 && report.SceneDependencyEdges.Count == 0)
            {
                findings.Add("Scene scripts were found, but no serialized script references were detected.");
                suggestions.Add("Confirm scene references are intentionally discovered at runtime rather than missing in the Inspector.");
            }

            var criticalSceneIssues = report.SceneWiringIssues.Count(issue => issue.Severity == AnalysisSeverity.Critical);
            var warningSceneIssues = report.SceneWiringIssues.Count(issue => issue.Severity == AnalysisSeverity.Warning);
            var infoSceneIssues = report.SceneWiringIssues.Count(issue => issue.Severity == AnalysisSeverity.Info);
            var score = 100 - duplicateManagers.Count * 14 - sceneCycles.Count * 10 - criticalSceneIssues * 16 - warningSceneIssues * 7 - infoSceneIssues * 2 - (report.SceneScriptNodes.Count > 0 && report.SceneDependencyEdges.Count == 0 ? 8 : 0);
            report.SetSceneHealth(new SceneHealthReport(score, report.SceneScriptNodes.Count, report.SceneDependencyEdges.Count, findings, suggestions));
        }

        private static bool IsManagerType(string typeName)
        {
            return typeName.Contains("Manager") || typeName.Contains("Service") || typeName.Contains("Controller");
        }
    }
}
