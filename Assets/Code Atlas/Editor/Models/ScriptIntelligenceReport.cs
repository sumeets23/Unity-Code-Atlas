using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptIntelligenceReport
    {
        [SerializeField] private string generatedAtUtc;
        [SerializeField] private List<UpdateMethodInfo> updateMethods = new List<UpdateMethodInfo>();
        [SerializeField] private List<ScriptIssue> issues = new List<ScriptIssue>();
        [SerializeField] private List<ScriptGraphNode> scriptGraphNodes = new List<ScriptGraphNode>();
        [SerializeField] private List<ScriptRelationshipEdge> scriptRelationshipEdges = new List<ScriptRelationshipEdge>();
        [SerializeField] private List<SceneScriptNode> sceneScriptNodes = new List<SceneScriptNode>();
        [SerializeField] private List<SceneDependencyEdge> sceneDependencyEdges = new List<SceneDependencyEdge>();
        [SerializeField] private List<SceneWiringIssue> sceneWiringIssues = new List<SceneWiringIssue>();
        [SerializeField] private List<DependencyEdge> dependencyEdges = new List<DependencyEdge>();
        [SerializeField] private List<DependencyCycle> dependencyCycles = new List<DependencyCycle>();
        [SerializeField] private ArchitectureHealthReport architectureHealth = new ArchitectureHealthReport();
        [SerializeField] private List<CouplingMetric> couplingMetrics = new List<CouplingMetric>();
        [SerializeField] private TechnicalDebtReport technicalDebt = new TechnicalDebtReport();
        [SerializeField] private SceneHealthReport sceneHealth = new SceneHealthReport();
        [SerializeField] private List<PerformanceFinding> performanceFindings = new List<PerformanceFinding>();
        [SerializeField] private List<PlatformPerformanceProfile> platformProfiles = new List<PlatformPerformanceProfile>();
        [SerializeField] private ArchitectureAssistantResult architectureAssistant = new ArchitectureAssistantResult();
        [SerializeField] private GraphExportSnapshot graphSnapshot = new GraphExportSnapshot();
        [SerializeField] private List<AnalyzedMethodInfo> analyzedMethods = new List<AnalyzedMethodInfo>();

        public string GeneratedAtUtc => generatedAtUtc;
        public List<UpdateMethodInfo> UpdateMethods => updateMethods;
        public List<ScriptIssue> Issues => issues;
        public List<ScriptGraphNode> ScriptGraphNodes => scriptGraphNodes;
        public List<ScriptRelationshipEdge> ScriptRelationshipEdges => scriptRelationshipEdges;
        public List<SceneScriptNode> SceneScriptNodes => sceneScriptNodes;
        public List<SceneDependencyEdge> SceneDependencyEdges => sceneDependencyEdges;
        public List<SceneWiringIssue> SceneWiringIssues => sceneWiringIssues;
        public List<DependencyEdge> DependencyEdges => dependencyEdges;
        public List<DependencyCycle> DependencyCycles => dependencyCycles;
        public ArchitectureHealthReport ArchitectureHealth => architectureHealth;
        public List<CouplingMetric> CouplingMetrics => couplingMetrics;
        public TechnicalDebtReport TechnicalDebt => technicalDebt;
        public SceneHealthReport SceneHealth => sceneHealth;
        public List<PerformanceFinding> PerformanceFindings => performanceFindings;
        public List<PlatformPerformanceProfile> PlatformProfiles => platformProfiles;
        public ArchitectureAssistantResult ArchitectureAssistant => architectureAssistant;
        public GraphExportSnapshot GraphSnapshot => graphSnapshot;
        public List<AnalyzedMethodInfo> AnalyzedMethods => analyzedMethods;

        public ScriptIntelligenceReport()
        {
            generatedAtUtc = DateTime.UtcNow.ToString("O");
        }

        public void SetArchitectureHealth(ArchitectureHealthReport value)
        {
            architectureHealth = value ?? new ArchitectureHealthReport();
        }

        public void SetTechnicalDebt(TechnicalDebtReport value)
        {
            technicalDebt = value ?? new TechnicalDebtReport();
        }

        public void SetSceneHealth(SceneHealthReport value)
        {
            sceneHealth = value ?? new SceneHealthReport();
        }

        public void SetArchitectureAssistantResult(ArchitectureAssistantResult value)
        {
            architectureAssistant = value ?? new ArchitectureAssistantResult();
        }
    }
}
