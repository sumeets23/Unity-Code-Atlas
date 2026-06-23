namespace ScriptIntelligence.Editor.ExecutionFlow
{
    public sealed class FlowGraphEdge
    {
        public string FromId { get; }
        public string ToId { get; }
        public FlowEventKind Kind { get; }
        public FlowRelationshipKind RelationshipKind { get; }
        public string Evidence { get; }
        public string AssetPath { get; }
        public int Line { get; }
        public int HitCount { get; set; }

        public FlowGraphEdge(string fromId, string toId, FlowEventKind kind)
            : this(fromId, toId, kind, FlowRelationshipKind.Unresolved, string.Empty, string.Empty, 1)
        {
        }

        public FlowGraphEdge(string fromId, string toId, FlowEventKind kind, FlowRelationshipKind relationshipKind, string evidence, string assetPath, int line)
        {
            FromId = fromId;
            ToId = toId;
            Kind = kind;
            RelationshipKind = relationshipKind;
            Evidence = evidence;
            AssetPath = assetPath;
            Line = line;
        }
    }
}
