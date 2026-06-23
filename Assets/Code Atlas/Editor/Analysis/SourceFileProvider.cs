using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class SourceFileProvider
    {
        public List<SourceFile> LoadProjectScripts()
        {
            var files = new List<SourceFile>();
            var root = Application.dataPath;
            var paths = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);

            foreach (var absolutePath in paths)
            {
                if (absolutePath.Contains("/Editor/ScriptIntelligence/Generated/"))
                {
                    continue;
                }

                var normalized = absolutePath.Replace('\\', '/');
                var assetPath = "Assets" + normalized.Substring(root.Replace('\\', '/').Length);
                if (assetPath.Contains("/Editor/"))
                {
                    continue;
                }

                files.Add(new SourceFile(assetPath, File.ReadAllText(absolutePath)));
            }

            return files;
        }

        public List<SourceFile> LoadOpenSceneScripts()
        {
            var files = new List<SourceFile>();
            var seenPaths = new HashSet<string>();
            var behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null || !behaviour.gameObject.scene.IsValid())
                {
                    continue;
                }

                var script = MonoScript.FromMonoBehaviour(behaviour);
                if (script == null || script.GetClass() == null)
                {
                    continue;
                }

                var assetPath = AssetDatabase.GetAssetPath(script);
                if (string.IsNullOrEmpty(assetPath) || assetPath.Contains("/Editor/") || !seenPaths.Add(assetPath))
                {
                    continue;
                }

                var absolutePath = Path.GetFullPath(assetPath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                files.Add(new SourceFile(assetPath, File.ReadAllText(absolutePath)));
            }

            return files;
        }
    }
}
