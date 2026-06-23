using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class AnalyzedMethodInfo
    {
        [SerializeField] private string className;
        [SerializeField] private string methodName;
        [SerializeField] private string body;
        [SerializeField] private int startLine;
        [SerializeField] private int endLine;

        public string ClassName => className;
        public string MethodName => methodName;
        public string Body => body;
        public int StartLine => startLine;
        public int EndLine => endLine;

        public AnalyzedMethodInfo(string className, string methodName, string body, int startLine, int endLine)
        {
            this.className = className;
            this.methodName = methodName;
            this.body = body;
            this.startLine = startLine;
            this.endLine = endLine;
        }
    }
}
