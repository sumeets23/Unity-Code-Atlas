# Script Intelligence Roadmap

## Highest-value additions

1. Roslyn-backed semantic analysis
   - Replace the lightweight scanner with Roslyn symbols.
   - Detect exact method calls, interface implementations, boxing, allocations, and call chains.
   - Map scene references to the exact methods that use the referenced field.

2. Scene wiring health
   - Show unassigned serialized references.
   - Detect duplicate manager components.
   - Flag scene references crossing additive scene boundaries.
   - Detect circular scene object references separately from static field cycles.

3. Profiler correlation
   - Record per-frame BehaviourUpdate, GC.Alloc, and scripts markers.
   - Correlate profiler spikes with scripts found by static analysis.
   - Keep a rolling timeline and mark spikes above configurable thresholds.

4. Fix suggestions
   - Generate suggested edits for common issues: cache GetComponent, remove empty Update, move allocations to Awake/Start.
   - Preview diffs before applying.
   - Support project-specific suppression comments.

5. Rule configuration
   - Enable/disable rules from the UI.
   - Per-project thresholds for long methods, max Update callbacks, and acceptable scene dependency depth.
   - Severity overrides for team coding standards.

6. Architecture views
   - Dependency graph filters by scene, assembly, namespace, and selected GameObject.
   - Highlight incoming/outgoing dependencies for a selected script.
   - Export graph snapshots for code reviews.

7. CI/reporting mode
   - Batchmode command to export JSON/Markdown without opening the window.
   - Exit-code thresholds for CI quality gates.
   - Historical report comparison to catch regressions.

## Best practical use cases

- Before performance reviews: scan for Update callbacks, allocations, logging, and scene-wide lookups.
- Before scene handoff: verify scene wiring and unassigned references.
- Before refactors: inspect script dependency fan-in/fan-out and cycles.
- During code review: export Markdown summaries with top findings and scene references.
- During optimization: correlate hot scripts with profiler samples and static warnings.
