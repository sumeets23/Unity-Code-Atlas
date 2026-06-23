using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScriptIntelligence.Editor.DependencyGraph
{
    public sealed class SceneDependencyGraphBuilder
    {
        public void Build(List<SceneScriptNode> sceneNodes, List<SceneDependencyEdge> sceneEdges, List<SceneWiringIssue> sceneIssues)
        {
            var seenNodes = new HashSet<string>();
            var seenEdges = new HashSet<string>();
            var behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
            DetectMissingComponents(sceneIssues);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null || behaviour.gameObject.scene.name == null)
                {
                    continue;
                }

                var fromType = behaviour.GetType().Name;
                var fromObjectPath = GetHierarchyPath(behaviour.transform);
                var scenePath = GetSceneLabel(behaviour.gameObject.scene);
                var nodeKey = fromType + "|" + scenePath + "|" + fromObjectPath;

                if (seenNodes.Add(nodeKey))
                {
                    sceneNodes.Add(new SceneScriptNode(fromType, scenePath, fromObjectPath));
                }

                var serializedObject = new SerializedObject(behaviour);
                var property = serializedObject.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyPath == "m_Script")
                    {
                        continue;
                    }

                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    var targetBehaviour = property.objectReferenceValue as MonoBehaviour;
                    if (targetBehaviour == null)
                    {
                        if (IsLikelySerializedReference(property))
                        {
                            sceneIssues.Add(new SceneWiringIssue(
                                "Null Serialized Field",
                                scenePath,
                                fromObjectPath,
                                fromType,
                                property.propertyPath,
                                "Serialized object reference is empty.",
                                "Assign the reference in the Inspector or remove the serialized field if runtime lookup is intentional.",
                                AnalysisSeverity.Warning));
                        }

                        continue;
                    }

                    if (targetBehaviour == behaviour)
                    {
                        continue;
                    }

                    var toType = targetBehaviour.GetType().Name;
                    var toObjectPath = GetHierarchyPath(targetBehaviour.transform);
                    var toScenePath = GetSceneLabel(targetBehaviour.gameObject.scene);
                    if (!targetBehaviour.gameObject.activeInHierarchy)
                    {
                        sceneIssues.Add(new SceneWiringIssue(
                            "Inactive Reference",
                            scenePath,
                            fromObjectPath,
                            fromType,
                            property.propertyPath,
                            "Serialized reference points to an inactive GameObject: " + toObjectPath,
                            "Confirm the target is intentionally inactive or move the dependency behind an activation service.",
                            AnalysisSeverity.Info));
                    }

                    if (toScenePath != scenePath)
                    {
                        sceneIssues.Add(new SceneWiringIssue(
                            "Cross Scene Dependency",
                            scenePath,
                            fromObjectPath,
                            fromType,
                            property.propertyPath,
                            "Serialized reference crosses scene boundary to " + toScenePath + ".",
                            "Replace cross-scene references with a scene loading contract, event channel, or runtime resolver.",
                            AnalysisSeverity.Warning));
                    }

                    var targetNodeKey = toType + "|" + toScenePath + "|" + toObjectPath;

                    if (seenNodes.Add(targetNodeKey))
                    {
                        sceneNodes.Add(new SceneScriptNode(toType, toScenePath, toObjectPath));
                    }

                    var edgeKey = fromType + "|" + toType + "|" + property.propertyPath + "|" + fromObjectPath + "|" + toObjectPath;
                    if (seenEdges.Add(edgeKey))
                    {
                        sceneEdges.Add(new SceneDependencyEdge(fromType, toType, property.propertyPath, fromObjectPath, toObjectPath, scenePath));
                    }
                }
            }
        }

        private static void DetectMissingComponents(List<SceneWiringIssue> sceneIssues)
        {
            var gameObjects = Object.FindObjectsOfType<GameObject>(true);
            foreach (var gameObject in gameObjects)
            {
                if (!gameObject.scene.IsValid())
                {
                    continue;
                }

                var components = gameObject.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component != null)
                    {
                        continue;
                    }

                    sceneIssues.Add(new SceneWiringIssue(
                        "Missing Component",
                        GetSceneLabel(gameObject.scene),
                        GetHierarchyPath(gameObject.transform),
                        "Missing Script",
                        string.Empty,
                        "GameObject contains a missing MonoBehaviour component.",
                        "Remove the missing component or restore the script GUID it references.",
                        AnalysisSeverity.Critical));
                }
            }
        }

        private static bool IsLikelySerializedReference(SerializedProperty property)
        {
            return property.propertyPath != "m_GameObject" && property.propertyPath != "m_PrefabAsset" && property.propertyPath != "m_PrefabInstance";
        }

        private static string GetSceneLabel(Scene scene)
        {
            if (!string.IsNullOrEmpty(scene.path))
            {
                return scene.path;
            }

            return string.IsNullOrEmpty(scene.name) ? "Untitled Scene" : scene.name;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            var current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names.ToArray());
        }
    }
}
