# Script Intelligence Editor Tool

Open from `Tools > Script Intelligence > Open`.

## Architecture

- `ScriptIntelligenceWindow` owns EditorWindow lifecycle, toolbar actions, tab state, and high-level coordination only.
- `Analysis` contains project source discovery, lightweight C# structure parsing, and the scan pipeline.
- `Analysis/Rules` contains independent `IScriptAnalysisRule` implementations. Add new optimization rules here.
- `DependencyGraph` builds a node graph for attachable `MonoBehaviour` scripts outside `Editor` folders. It includes public, private, protected, internal, serialized, and non-serialized fields, then detects cycles.
- `Profiling` owns `ProfilerRecorder` lifetime and runtime metric sampling.
- `Reporting` exports scan data.
- `Models` contains serializable result types shared by UI, reports, and services.
- `UI` contains one renderer class per panel.
- `Utilities` contains editor helpers such as source navigation.

The current parser is dependency-free and intentionally isolated behind `CSharpStructureScanner`. Replace that class with a Roslyn-backed implementation when the project adds Roslyn assemblies.
