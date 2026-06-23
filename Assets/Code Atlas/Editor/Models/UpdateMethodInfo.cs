using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class UpdateMethodInfo
    {
        [SerializeField] private string className;
        [SerializeField] private string methodName;
        [SerializeField] private string assetPath;
        [SerializeField] private int line;

        public string ClassName => className;
        public string MethodName => methodName;
        public string AssetPath => assetPath;
        public int Line => line;

        public UpdateMethodInfo(string className, string methodName, string assetPath, int line)
        {
            this.className = className;
            this.methodName = methodName;
            this.assetPath = assetPath;
            this.line = line;
        }
    }
}
