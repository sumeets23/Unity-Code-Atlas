namespace ScriptIntelligence.Editor.ExecutionFlow
{
    public sealed class FlowGraphNode
    {
        public string Id { get; }
        public string ScriptName { get; }
        public string MethodName { get; }
        public string AssetPath { get; }
        public int Line { get; }
        public bool IsUnityMessage { get; }
        public int HitCount { get; set; }
        public int FirstFrame { get; set; }
        public int LastFrame { get; set; }

        public FlowGraphNode(string scriptName, string methodName)
            : this(scriptName, methodName, string.Empty, 1, false)
        {
        }

        public FlowGraphNode(string scriptName, string methodName, string assetPath, int line, bool isUnityMessage)
        {
            ScriptName = scriptName;
            MethodName = methodName;
            AssetPath = assetPath;
            Line = line;
            IsUnityMessage = isUnityMessage;
            Id = scriptName + "." + methodName;
            FirstFrame = int.MaxValue;
        }
    }
}
