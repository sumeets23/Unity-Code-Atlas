# Script Intelligence Studio Architecture

## Module Dependency Map

```mermaid
flowchart TD
    Window["ScriptIntelligenceWindow"] --> Pipeline["ScriptAnalysisPipeline"]
    Window --> Panels["IAnalysisPanel implementations"]
    Window --> Profiler["ScriptProfilerRecorderService"]
    Window --> Exporter["ReportExporter"]

    Pipeline --> Rules["IScriptAnalysisRule rules"]
    Pipeline --> Graph["DependencyGraphBuilder"]
    Pipeline --> SceneGraph["SceneDependencyGraphBuilder"]
    Pipeline --> Analyzers["IScriptIntelligenceAnalyzer analyzers"]

    Analyzers --> Coupling["CouplingAnalyzer"]
    Analyzers --> SceneHealth["SceneHealthScanner"]
    Analyzers --> Performance["PerformanceIntelligenceAnalyzer"]
    Analyzers --> Debt["TechnicalDebtAnalyzer"]
    Analyzers --> Health["ArchitectureHealthAnalyzer"]
    Analyzers --> Assistant["ArchitectureInsightEngine"]

    Assistant --> Provider["IArchitectureAssistant"]
    Provider --> Local["LocalArchitectureAssistant"]

    Profiler --> RuntimeFlow["RuntimeGraphInstrumentationSystem"]
    Profiler --> Replay["ArchitectureReplaySession"]

    Panels --> Models["ScriptIntelligenceReport + DTOs"]
    Exporter --> Models
    Pipeline --> Models
```

## Analyzer Class Diagram

```mermaid
classDiagram
    class IScriptIntelligenceAnalyzer {
        <<interface>>
        +Analyze(context, report) void
    }

    class CouplingAnalyzer
    class SceneHealthScanner
    class PerformanceIntelligenceAnalyzer
    class TechnicalDebtAnalyzer
    class ArchitectureHealthAnalyzer
    class ArchitectureInsightEngine

    IScriptIntelligenceAnalyzer <|.. CouplingAnalyzer
    IScriptIntelligenceAnalyzer <|.. SceneHealthScanner
    IScriptIntelligenceAnalyzer <|.. PerformanceIntelligenceAnalyzer
    IScriptIntelligenceAnalyzer <|.. TechnicalDebtAnalyzer
    IScriptIntelligenceAnalyzer <|.. ArchitectureHealthAnalyzer
    IScriptIntelligenceAnalyzer <|.. ArchitectureInsightEngine

    class IArchitectureAssistant {
        <<interface>>
        +GenerateInsights(context) ArchitectureAssistantResult
    }

    class LocalArchitectureAssistant
    IArchitectureAssistant <|.. LocalArchitectureAssistant
    ArchitectureInsightEngine --> IArchitectureAssistant
```

## Runtime Replay Flow

```mermaid
sequenceDiagram
    participant User
    participant Window as ScriptIntelligenceWindow
    participant Profiler as ScriptProfilerRecorderService
    participant Runtime as RuntimeGraphInstrumentationSystem
    participant Replay as ArchitectureReplaySession

    User->>Window: Start Recording
    Window->>Profiler: Start()
    Profiler->>Runtime: Start()
    Runtime->>Runtime: Sample active MonoBehaviours
    Runtime->>Replay: Add RuntimeExecutionEvent
    User->>Window: Scrub Timeline
    Window->>Replay: Read current event
```

## Extension Points

- Add static rules by implementing `IScriptAnalysisRule`.
- Add report enrichment by implementing `IScriptIntelligenceAnalyzer`.
- Add AI providers by implementing `IArchitectureAssistant`.
- Add visual surfaces by implementing `IAnalysisPanel`.
