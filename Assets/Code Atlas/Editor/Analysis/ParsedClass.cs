namespace ScriptIntelligence.Editor.Analysis
{
    using System.Collections.Generic;
    using ScriptIntelligence.Editor.Models;

    public sealed class ParsedClass
    {
        public string ClassName { get; }
        public string AssetPath { get; }
        public int Line { get; }
        public bool IsMonoBehaviour { get; }
        public bool IsScriptableObject { get; }
        public bool IsInterface { get; }
        public bool IsAbstract { get; }
        public ScriptNodeType NodeType { get; }
        public IReadOnlyList<string> BaseTypes { get; }

        public ParsedClass(string className, string assetPath, int line, bool isMonoBehaviour, IReadOnlyList<string> baseTypes)
            : this(className, assetPath, line, isMonoBehaviour, false, false, false, baseTypes)
        {
        }

        public ParsedClass(string className, string assetPath, int line, bool isMonoBehaviour, bool isScriptableObject, bool isInterface, bool isAbstract, IReadOnlyList<string> baseTypes)
        {
            ClassName = className;
            AssetPath = assetPath;
            Line = line;
            IsMonoBehaviour = isMonoBehaviour;
            IsScriptableObject = isScriptableObject;
            IsInterface = isInterface;
            IsAbstract = isAbstract;
            BaseTypes = baseTypes;
            NodeType = ClassifyNodeType(className, isMonoBehaviour, isScriptableObject, isInterface, isAbstract);
        }

        private static ScriptNodeType ClassifyNodeType(string className, bool isMonoBehaviour, bool isScriptableObject, bool isInterface, bool isAbstract)
        {
            if (isInterface)
            {
                return ScriptNodeType.Interface;
            }

            if (className.Contains("Singleton"))
            {
                return ScriptNodeType.Singleton;
            }

            if (className.Contains("Manager") || className.Contains("Service") || className.Contains("Controller"))
            {
                return ScriptNodeType.Manager;
            }

            if (isMonoBehaviour)
            {
                return ScriptNodeType.MonoBehaviour;
            }

            if (isScriptableObject)
            {
                return ScriptNodeType.ScriptableObject;
            }

            if (isAbstract)
            {
                return ScriptNodeType.AbstractClass;
            }

            return ScriptNodeType.PlainClass;
        }
    }
}
