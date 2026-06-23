using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis.Performance
{
    public sealed class PerformanceIntelligenceAnalyzer : IScriptIntelligenceAnalyzer
    {
        public void Analyze(ScriptIntelligenceAnalysisContext context, ScriptIntelligenceReport report)
        {
            report.PerformanceFindings.Clear();
            report.PlatformProfiles.Clear();
            report.PlatformProfiles.AddRange(BuildProfiles());

            foreach (var profile in report.PlatformProfiles)
            {
                AddFindings(report, profile);
            }
        }

        private static void AddFindings(ScriptIntelligenceReport report, PlatformPerformanceProfile platformProfile)
        {
            foreach (var issueGroup in report.Issues.GroupBy(issue => MapCategory(issue.RuleId)))
            {
                if (issueGroup.Key == "General")
                {
                    continue;
                }

                var count = issueGroup.Count();
                var severity = issueGroup.Any(issue => issue.Severity == AnalysisSeverity.Critical) || count >= platformProfile.CriticalThreshold
                    ? AnalysisSeverity.Critical
                    : count >= platformProfile.WarningThreshold ? AnalysisSeverity.Warning : AnalysisSeverity.Info;

                report.PerformanceFindings.Add(new PerformanceFinding(
                    issueGroup.Key,
                    issueGroup.Key + " risk detected",
                    BuildRecommendation(issueGroup.Key, platformProfile, count),
                    platformProfile.Name,
                    severity));
            }
        }

        private static IEnumerable<PlatformPerformanceProfile> BuildProfiles()
        {
            yield return new PlatformPerformanceProfile("Mobile", 1, 2, "Prefer cached references, pooled collections, and minimal per-frame CPU work.");
            yield return new PlatformPerformanceProfile("PC", 2, 5, "Focus on obvious hotspots and keep allocations out of hot paths.");
            yield return new PlatformPerformanceProfile("VR", 1, 2, "Frame budget is strict; remove per-frame lookups, physics spikes, and allocations early.");
            yield return new PlatformPerformanceProfile("Console", 1, 3, "Avoid unpredictable allocations and consolidate expensive queries for stable frame pacing.");
        }

        private static string MapCategory(string ruleId)
        {
            if (ruleId == "SI-PERF-002")
            {
                return "Excessive Find Calls";
            }

            if (ruleId == "SI-PERF-005")
            {
                return "Raycast Abuse";
            }

            if (ruleId == "SI-PERF-006")
            {
                return "Physics Misuse";
            }

            if (ruleId == "SI-PERF-001")
            {
                return "Repeated GetComponent";
            }

            if (ruleId == "SI-GC-001")
            {
                return "Allocation Chains";
            }

            if (ruleId == "SI-GC-002")
            {
                return "Heavy LINQ";
            }

            if (ruleId == "SI-GC-003" || ruleId == "SI-GC-005")
            {
                return "String Allocations";
            }

            if (ruleId == "SI-GC-004" || ruleId == "SI-MEM-001")
            {
                return "Large Collections";
            }

            return "General";
        }

        private static string BuildRecommendation(string category, PlatformPerformanceProfile platformProfile, int count)
        {
            return category + " appears in " + count + " findings. For " + platformProfile.Name + ", " + platformProfile.Guidance;
        }
    }
}
