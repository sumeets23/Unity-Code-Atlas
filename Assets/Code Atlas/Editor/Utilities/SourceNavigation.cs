using UnityEditor;
using UnityEngine;

namespace ScriptIntelligence.Editor.Utilities
{
    public static class SourceNavigation
    {
        public static void Open(string assetPath, int line)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning("Unable to open source file: " + assetPath);
                return;
            }

            AssetDatabase.OpenAsset(asset, Mathf.Max(1, line));
        }
    }
}
