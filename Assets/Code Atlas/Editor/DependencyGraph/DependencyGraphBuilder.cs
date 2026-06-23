using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Analysis;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.DependencyGraph
{
    public sealed class DependencyGraphBuilder
    {
        private static readonly HashSet<string> BuiltInTypes = new HashSet<string>
        {
            "int", "float", "double", "bool", "string", "Vector2", "Vector3", "Vector4", "Quaternion", "Color", "Transform", "GameObject"
        };

        public void Build(
            IReadOnlyList<ParsedClass> classes,
            IReadOnlyList<ParsedMethod> methods,
            IReadOnlyList<ParsedField> fields,
            List<ScriptGraphNode> scriptNodes,
            List<ScriptRelationshipEdge> relationshipEdges,
            List<DependencyEdge> edges,
            List<DependencyCycle> cycles)
        {
            var architectureClasses = classes.Where(IsArchitectureType).ToList();
            var projectTypes = new HashSet<string>(architectureClasses.Select(type => type.ClassName));
            var nodesByType = new Dictionary<string, ScriptGraphNode>();

            foreach (var parsedClass in architectureClasses)
            {
                if (nodesByType.ContainsKey(parsedClass.ClassName))
                {
                    continue;
                }

                var node = new ScriptGraphNode(parsedClass.ClassName, parsedClass.AssetPath, parsedClass.Line, parsedClass.NodeType);
                nodesByType.Add(parsedClass.ClassName, node);
                scriptNodes.Add(node);
            }

            BuildInheritanceRelationships(architectureClasses, projectTypes, relationshipEdges);

            foreach (var method in methods)
            {
                if (!nodesByType.TryGetValue(method.ClassName, out var node))
                {
                    continue;
                }

                node.Methods.Add(new ScriptMethodNode(
                    method.MethodName,
                    ParseVisibility(method.Visibility),
                    method.StartLine,
                    method.EndLine,
                    IsUnityMessage(method.MethodName),
                    method.Body));
            }

            foreach (var field in fields)
            {
                if (!nodesByType.TryGetValue(field.DeclaringType, out var node))
                {
                    continue;
                }

                var targetType = StripGenericContainer(field.FieldType);
                var isProjectScriptReference = !BuiltInTypes.Contains(targetType) && projectTypes.Contains(targetType);
                node.Fields.Add(new ScriptFieldNode(
                    field.FieldName,
                    field.FieldType,
                    ParseVisibility(field.Visibility),
                    field.Line,
                    field.Serialized,
                    isProjectScriptReference));

                if (!isProjectScriptReference)
                {
                    continue;
                }

                edges.Add(new DependencyEdge(field.DeclaringType, targetType, field.FieldName, field.AssetPath, field.Line));
                relationshipEdges.Add(new ScriptRelationshipEdge(
                    field.DeclaringType,
                    targetType,
                    ScriptRelationshipType.SerializedField,
                    field.FieldName,
                    string.Empty,
                    field.AssetPath,
                    field.Line,
                    field.Visibility + " " + field.FieldType + " " + field.FieldName));
            }

            BuildMethodRelationships(methods, fields, projectTypes, relationshipEdges);
            FindCycles(edges, cycles);
        }

        private static bool IsArchitectureType(ParsedClass parsedClass)
        {
            return parsedClass.IsMonoBehaviour ||
                   parsedClass.IsScriptableObject ||
                   parsedClass.IsInterface ||
                   parsedClass.IsAbstract ||
                   parsedClass.ClassName.Contains("Manager") ||
                   parsedClass.ClassName.Contains("Service") ||
                   parsedClass.ClassName.Contains("Controller") ||
                   parsedClass.ClassName.Contains("Singleton");
        }

        private static void BuildInheritanceRelationships(IReadOnlyList<ParsedClass> classes, HashSet<string> projectTypes, List<ScriptRelationshipEdge> relationshipEdges)
        {
            foreach (var parsedClass in classes)
            {
                foreach (var baseType in parsedClass.BaseTypes)
                {
                    if (baseType == "MonoBehaviour" || baseType == "Behaviour" || baseType == "Component")
                    {
                        continue;
                    }

                    var relationshipType = baseType.StartsWith("I") ? ScriptRelationshipType.InterfaceImplementation : ScriptRelationshipType.Inheritance;
                    if (!projectTypes.Contains(baseType) && relationshipType == ScriptRelationshipType.Inheritance)
                    {
                        continue;
                    }

                    relationshipEdges.Add(new ScriptRelationshipEdge(
                        parsedClass.ClassName,
                        baseType,
                        relationshipType,
                        baseType,
                        string.Empty,
                        parsedClass.AssetPath,
                        parsedClass.Line,
                        parsedClass.ClassName + " : " + baseType));
                }
            }
        }

        private static void BuildMethodRelationships(IReadOnlyList<ParsedMethod> methods, IReadOnlyList<ParsedField> fields, HashSet<string> projectTypes, List<ScriptRelationshipEdge> relationshipEdges)
        {
            var fieldsByType = fields.GroupBy(field => field.DeclaringType).ToDictionary(group => group.Key, group => group.ToList());
            var emitted = new HashSet<string>();

            foreach (var method in methods)
            {
                if (!fieldsByType.TryGetValue(method.ClassName, out var declaringFields))
                {
                    declaringFields = new List<ParsedField>();
                }

                foreach (var field in declaringFields)
                {
                    var targetType = StripGenericContainer(field.FieldType);
                    if (!projectTypes.Contains(targetType))
                    {
                        continue;
                    }

                    if (method.Body.Contains(field.FieldName + "."))
                    {
                        AddRelationshipOnce(
                            relationshipEdges,
                            emitted,
                            method.ClassName,
                            targetType,
                            ScriptRelationshipType.MethodCall,
                            field.FieldName,
                            method.MethodName,
                            field.AssetPath,
                            method.StartLine,
                            method.MethodName + " uses " + field.FieldName);
                    }

                    if (method.Body.Contains(field.FieldName + ".") && (method.Body.Contains("+=") || method.Body.Contains("-=")))
                    {
                        AddRelationshipOnce(
                            relationshipEdges,
                            emitted,
                            method.ClassName,
                            targetType,
                            ScriptRelationshipType.EventSubscription,
                            field.FieldName,
                            method.MethodName,
                            field.AssetPath,
                            method.StartLine,
                            method.MethodName + " subscribes through " + field.FieldName);
                    }
                }

                foreach (var projectType in projectTypes)
                {
                    if (projectType == method.ClassName)
                    {
                        continue;
                    }

                    if (method.Body.Contains(projectType + ".Instance") || method.Body.Contains(projectType + ".instance"))
                    {
                        AddRelationshipOnce(
                            relationshipEdges,
                            emitted,
                            method.ClassName,
                            projectType,
                            ScriptRelationshipType.SingletonUsage,
                            "Instance",
                            method.MethodName,
                            string.Empty,
                            method.StartLine,
                            method.MethodName + " uses " + projectType + ".Instance");
                    }
                }
            }
        }

        private static void AddRelationshipOnce(List<ScriptRelationshipEdge> relationships, HashSet<string> emitted, string fromType, string toType, ScriptRelationshipType relationshipType, string memberName, string methodName, string assetPath, int line, string evidence)
        {
            var key = BuildRelationshipKey(fromType, toType, relationshipType, memberName, methodName, line);
            if (!emitted.Add(key))
            {
                return;
            }

            relationships.Add(new ScriptRelationshipEdge(fromType, toType, relationshipType, memberName, methodName, assetPath, line, evidence));
        }

        private static string BuildRelationshipKey(string fromType, string toType, ScriptRelationshipType relationshipType, string memberName, string methodName, int line)
        {
            switch (relationshipType)
            {
                case ScriptRelationshipType.MethodCall:
                case ScriptRelationshipType.EventSubscription:
                case ScriptRelationshipType.SingletonUsage:
                    return fromType + "|" + toType + "|" + relationshipType + "|" + methodName;
                default:
                    return fromType + "|" + toType + "|" + relationshipType + "|" + memberName + "|" + methodName + "|" + line;
            }
        }

        private static ScriptMemberVisibility ParseVisibility(string visibility)
        {
            switch (visibility)
            {
                case "public":
                    return ScriptMemberVisibility.Public;
                case "protected":
                    return ScriptMemberVisibility.Protected;
                case "internal":
                    return ScriptMemberVisibility.Internal;
                default:
                    return ScriptMemberVisibility.Private;
            }
        }

        private static bool IsUnityMessage(string methodName)
        {
            switch (methodName)
            {
                case "Awake":
                case "OnEnable":
                case "Start":
                case "Update":
                case "FixedUpdate":
                case "LateUpdate":
                case "OnDisable":
                case "OnDestroy":
                case "OnGUI":
                    return true;
                default:
                    return false;
            }
        }

        private static void FindCycles(IReadOnlyList<DependencyEdge> edges, List<DependencyCycle> cycles)
        {
            var adjacency = new Dictionary<string, List<string>>();
            foreach (var edge in edges)
            {
                if (!adjacency.TryGetValue(edge.FromType, out var targets))
                {
                    targets = new List<string>();
                    adjacency.Add(edge.FromType, targets);
                }

                targets.Add(edge.ToType);
            }

            var visited = new HashSet<string>();
            var stack = new Stack<string>();
            var inStack = new HashSet<string>();
            var emitted = new HashSet<string>();

            foreach (var node in adjacency.Keys)
            {
                Visit(node, adjacency, visited, stack, inStack, emitted, cycles);
            }
        }

        private static void Visit(
            string node,
            IReadOnlyDictionary<string, List<string>> adjacency,
            HashSet<string> visited,
            Stack<string> stack,
            HashSet<string> inStack,
            HashSet<string> emitted,
            List<DependencyCycle> cycles)
        {
            if (inStack.Contains(node))
            {
                var path = stack.Reverse().SkipWhile(item => item != node).Concat(new[] { node }).ToList();
                var key = string.Join(">", path);
                if (emitted.Add(key))
                {
                    cycles.Add(new DependencyCycle(path));
                }

                return;
            }

            if (!visited.Add(node))
            {
                return;
            }

            stack.Push(node);
            inStack.Add(node);

            if (adjacency.TryGetValue(node, out var targets))
            {
                foreach (var target in targets)
                {
                    Visit(target, adjacency, visited, stack, inStack, emitted, cycles);
                }
            }

            inStack.Remove(node);
            stack.Pop();
        }

        private static string StripGenericContainer(string typeName)
        {
            var clean = typeName.Trim();
            var genericStart = clean.IndexOf('<');
            if (genericStart < 0)
            {
                return clean;
            }

            var genericEnd = clean.LastIndexOf('>');
            if (genericEnd <= genericStart)
            {
                return clean;
            }

            return clean.Substring(genericStart + 1, genericEnd - genericStart - 1).Split(',')[0].Trim();
        }
    }
}


