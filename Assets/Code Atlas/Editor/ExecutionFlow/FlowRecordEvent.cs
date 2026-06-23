using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.ExecutionFlow
{
    [Serializable]
    public sealed class FlowRecordEvent
    {
        [SerializeField] private FlowEventKind kind;
        [SerializeField] private string scriptName;
        [SerializeField] private string methodName;
        [SerializeField] private string targetScriptName;
        [SerializeField] private string targetMethodName;
        [SerializeField] private int frame;
        [SerializeField] private double timestamp;

        public FlowEventKind Kind => kind;
        public string ScriptName => scriptName;
        public string MethodName => methodName;
        public string TargetScriptName => targetScriptName;
        public string TargetMethodName => targetMethodName;
        public int Frame => frame;
        public double Timestamp => timestamp;

        public FlowRecordEvent(FlowEventKind kind, string scriptName, string methodName, string targetScriptName, string targetMethodName, int frame, double timestamp)
        {
            this.kind = kind;
            this.scriptName = scriptName;
            this.methodName = methodName;
            this.targetScriptName = targetScriptName;
            this.targetMethodName = targetMethodName;
            this.frame = frame;
            this.timestamp = timestamp;
        }
    }
}
