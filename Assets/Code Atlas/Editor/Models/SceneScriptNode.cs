using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class SceneScriptNode
    {
        [SerializeField] private string typeName;
        [SerializeField] private string scenePath;
        [SerializeField] private string gameObjectPath;

        public string TypeName => typeName;
        public string ScenePath => scenePath;
        public string GameObjectPath => gameObjectPath;

        public SceneScriptNode(string typeName, string scenePath, string gameObjectPath)
        {
            this.typeName = typeName;
            this.scenePath = scenePath;
            this.gameObjectPath = gameObjectPath;
        }
    }
}
