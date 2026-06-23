using System.Collections.Generic;
using ScriptIntelligence.Editor.AI.Local;
using ScriptIntelligence.Editor.Analysis.Architecture;
using ScriptIntelligence.Editor.Analysis.Coupling;
using ScriptIntelligence.Editor.Analysis.Debt;
using ScriptIntelligence.Editor.Analysis.Health;
using ScriptIntelligence.Editor.Analysis.Performance;
using ScriptIntelligence.Editor.Analysis.Rules;
using ScriptIntelligence.Editor.Analysis.Scene;
using ScriptIntelligence.Editor.Core.Contracts;
using ScriptIntelligence.Editor.DependencyGraph;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class ScriptAnalysisPipeline
    {
        private readonly SourceFileProvider sourceFileProvider = new SourceFileProvider();
        private readonly CSharpStructureScanner structureScanner = new CSharpStructureScanner();
        private readonly DependencyGraphBuilder dependencyGraphBuilder = new DependencyGraphBuilder();
        private readonly SceneDependencyGraphBuilder sceneDependencyGraphBuilder = new SceneDependencyGraphBuilder();
        private readonly List<IScriptIntelligenceAnalyzer> analyzers = new List<IScriptIntelligenceAnalyzer>
        {
            new CouplingAnalyzer(),
            new SceneHealthScanner(),
            new PerformanceIntelligenceAnalyzer(),
            new TechnicalDebtAnalyzer(),
            new ArchitectureHealthAnalyzer(),
            new GraphExportSnapshotBuilder(),
            new ArchitectureInsightEngine(new LocalArchitectureAssistant())
        };
        private readonly List<IScriptAnalysisRule> rules = new List<IScriptAnalysisRule>
        {
            new AllocationInUpdateRule(),
            new LinqInUpdateRule(),
            new GetComponentInHotPathRule(),
            new FindObjectInHotPathRule(),
            new StringConcatInUpdateRule(),
            new DebugLogInHotPathRule(),
            new EmptyUpdateRule(),
            new LongMethodRule(),
            new RaycastInHotPathRule(),
            new PhysicsQueryAllocationRule(),
            new CollectionAllocationInHotPathRule(),
            new StringFormatInHotPathRule(),
            new LargeSerializedCollectionRule()
        };

        public ScriptIntelligenceReport Scan()
        {
            return ScanFiles(sourceFileProvider.LoadProjectScripts());
        }

        public ScriptIntelligenceReport ScanScene()
        {
            return ScanFiles(sourceFileProvider.LoadOpenSceneScripts());
        }

        private ScriptIntelligenceReport ScanFiles(List<SourceFile> files)
        {
            var report = new ScriptIntelligenceReport();
            var classes = new List<ParsedClass>();
            var methodsByFile = new Dictionary<SourceFile, List<ParsedMethod>>();
            var fields = new List<ParsedField>();

            foreach (var file in files)
            {
                classes.AddRange(structureScanner.FindClasses(file));
                var methods = structureScanner.FindMethods(file);
                methodsByFile[file] = methods;
                fields.AddRange(structureScanner.FindFields(file));

                foreach (var method in methods)
                {
                    if (method.MethodName == "Update" || method.MethodName == "FixedUpdate" || method.MethodName == "LateUpdate")
                    {
                        report.UpdateMethods.Add(new UpdateMethodInfo(method.ClassName, method.MethodName, file.AssetPath, method.StartLine));
                    }
                }

                foreach (var rule in rules)
                {
                    report.Issues.AddRange(rule.Analyze(file, methods));
                }
            }

            var allMethods = new List<ParsedMethod>();
            foreach (var fileMethods in methodsByFile.Values)
            {
                allMethods.AddRange(fileMethods);
            }

            foreach (var method in allMethods)
            {
                report.AnalyzedMethods.Add(new AnalyzedMethodInfo(method.ClassName, method.MethodName, method.Body, method.StartLine, method.EndLine));
            }

            dependencyGraphBuilder.Build(classes, allMethods, fields, report.ScriptGraphNodes, report.ScriptRelationshipEdges, report.DependencyEdges, report.DependencyCycles);
            sceneDependencyGraphBuilder.Build(report.SceneScriptNodes, report.SceneDependencyEdges, report.SceneWiringIssues);
            var context = new ScriptIntelligenceAnalysisContext(files, classes, allMethods, fields);
            foreach (var analyzer in analyzers)
            {
                analyzer.Analyze(context, report);
            }

            return report;
        }
    }
}
