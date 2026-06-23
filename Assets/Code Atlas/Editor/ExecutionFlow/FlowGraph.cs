using System.Collections.Generic;

namespace ScriptIntelligence.Editor.ExecutionFlow
{
    public sealed class FlowGraph
    {
        public List<FlowGraphNode> Nodes { get; } = new List<FlowGraphNode>();
        public List<FlowGraphEdge> Edges { get; } = new List<FlowGraphEdge>();
        public List<FlowRecordEvent> Events { get; } = new List<FlowRecordEvent>();

        public void Clear()
        {
            Nodes.Clear();
            Edges.Clear();
            Events.Clear();
        }
    }
}
