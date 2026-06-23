using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.ExecutionFlow
{
    public sealed class FlowGraphBuilder
    {
        private const int MaxDepth = 12;
        private static readonly Regex DirectCallRegex = new Regex(@"\b(?<method>[A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);
        private static readonly Regex MemberCallRegex = new Regex(@"\b(?<expression>[A-Za-z_][A-Za-z0-9_]*(?:\s*\.\s*[A-Za-z_][A-Za-z0-9_]*)*)\s*\.\s*(?<method>[A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);
        private static readonly Regex GenericResolverCallRegex = new Regex(@"\b(?:GetComponent|GetComponentInChildren|GetComponentInParent|FindObjectOfType|FindFirstObjectByType|FindAnyObjectByType)\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>\s*\(\s*\)\s*\.\s*(?<method>[A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);
        private static readonly Regex EventSubscriptionRegex = new Regex(@"(?:\+=|-=)\s*(?<method>[A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.Compiled);
        private static readonly HashSet<string> IgnoredCalls = new HashSet<string>
        {
            "if", "for", "foreach", "while", "switch", "return", "new", "throw", "catch", "using", "lock",
            "typeof", "nameof", "sizeof", "default", "Debug", "Log", "LogWarning", "LogError", "Assert",
            "GetComponent", "GetComponentInChildren", "GetComponentInParent", "TryGetComponent", "FindObjectOfType",
            "FindObjectsOfType", "FindFirstObjectByType", "FindAnyObjectByType", "StartCoroutine", "StopCoroutine",
            "StopAllCoroutines", "Instantiate", "Destroy", "WaitForSeconds", "WaitForEndOfFrame", "WaitUntil", "WaitWhile",
            "print", "SetActive", "ToString", "Equals", "GetHashCode"
        };

        public FlowGraph Build(string selectedScript, ScriptIntelligenceReport report, IReadOnlyList<FlowRecordEvent> recordedEvents)
        {
            var graph = new FlowGraph();
            foreach (var recordEvent in recordedEvents)
            {
                graph.Events.Add(recordEvent);
            }

            if (string.IsNullOrEmpty(selectedScript))
            {
                return graph;
            }

            var methodsByClass = report.AnalyzedMethods
                .GroupBy(method => method.ClassName)
                .ToDictionary(group => group.Key, group => group.OrderBy(method => method.StartLine).ToList());

            var fieldsByClass = report.ScriptGraphNodes
                .GroupBy(node => node.ClassName)
                .ToDictionary(group => group.Key, group => group.SelectMany(node => node.Fields).ToList());

            var scriptPaths = report.ScriptGraphNodes
                .GroupBy(node => node.ClassName)
                .ToDictionary(group => group.Key, group => group.First().AssetPath);

            var context = new BuildContext(methodsByClass, fieldsByClass, scriptPaths);
            BuildStaticFlow(graph, selectedScript, context);
            ApplyRecordedEvents(graph, recordedEvents, context);
            return graph;
        }

        private static void BuildStaticFlow(FlowGraph graph, string selectedScript, BuildContext context)
        {
            if (!context.MethodsByClass.TryGetValue(selectedScript, out var selectedMethods))
            {
                return;
            }

            var entryPoints = selectedMethods
                .Where(method => IsUnityMessage(method.MethodName))
                .OrderBy(method => UnityMessageOrder(method.MethodName))
                .ThenBy(method => method.StartLine)
                .ToList();

            foreach (var entry in entryPoints)
            {
                GetOrCreateNode(graph, entry, context);
                AddMethodChain(graph, entry, context, new HashSet<string>(), 0);
            }
        }

        private static void AddMethodChain(FlowGraph graph, AnalyzedMethodInfo method, BuildContext context, HashSet<string> path, int depth)
        {
            var methodKey = method.ClassName + "." + method.MethodName;
            if (depth > MaxDepth || path.Contains(methodKey))
            {
                return;
            }

            path.Add(methodKey);
            var source = GetOrCreateNode(graph, method, context);
            foreach (var call in DiscoverCalls(method, context))
            {
                if (call.RelationshipKind == FlowRelationshipKind.CrossScript)
                {
                    AddCrossScriptCall(graph, source, method, call, context, path, depth);
                }
                else
                {
                    AddSameScriptCall(graph, source, method, call, context, path, depth);
                }
            }

            path.Remove(methodKey);
        }

        private static void AddSameScriptCall(FlowGraph graph, FlowGraphNode source, AnalyzedMethodInfo sourceMethod, DiscoveredCall call, BuildContext context, HashSet<string> path, int depth)
        {
            var targetMethod = ResolveMethod(context.MethodsByClass, sourceMethod.ClassName, call.MethodName);
            if (targetMethod == null || targetMethod.MethodName == sourceMethod.MethodName)
            {
                return;
            }

            var target = GetOrCreateNode(graph, targetMethod, context);
            AddEdge(graph, source.Id, target.Id, call.EventKind, FlowRelationshipKind.SameScript, call.Evidence, AssetPathFor(context, sourceMethod.ClassName), call.Line);
            AddMethodChain(graph, targetMethod, context, path, depth + 1);
        }

        private static void AddCrossScriptCall(FlowGraph graph, FlowGraphNode source, AnalyzedMethodInfo sourceMethod, DiscoveredCall call, BuildContext context, HashSet<string> path, int depth)
        {
            var targetMethod = ResolveMethod(context.MethodsByClass, call.TargetScript, call.MethodName);
            if (targetMethod == null)
            {
                return;
            }

            var target = GetOrCreateNode(graph, targetMethod, context);
            AddEdge(graph, source.Id, target.Id, call.EventKind, FlowRelationshipKind.CrossScript, call.Evidence, AssetPathFor(context, sourceMethod.ClassName), call.Line);
            AddMethodChain(graph, targetMethod, context, path, depth + 1);
        }

        private static IEnumerable<DiscoveredCall> DiscoverCalls(AnalyzedMethodInfo method, BuildContext context)
        {
            var lines = method.Body.Split('\n');
            var blockComment = false;
            var emitted = new HashSet<string>();
            var localMethods = context.MethodsByClass.TryGetValue(method.ClassName, out var methodList)
                ? new HashSet<string>(methodList.Select(item => item.MethodName))
                : new HashSet<string>();

            for (var i = 0; i < lines.Length; i++)
            {
                var cleaned = StripCommentsAndStrings(lines[i], ref blockComment);
                if (string.IsNullOrWhiteSpace(cleaned) || LooksLikeDeclaration(cleaned))
                {
                    continue;
                }

                var line = method.StartLine + i;
                var occupiedRanges = new List<RangeSpan>();

                foreach (Match match in GenericResolverCallRegex.Matches(cleaned))
                {
                    var targetScript = CleanTypeName(match.Groups["type"].Value);
                    var methodName = match.Groups["method"].Value;
                    if (!CanResolve(context, targetScript, methodName))
                    {
                        continue;
                    }

                    occupiedRanges.Add(new RangeSpan(match.Index, match.Length));
                    var key = "resolver|" + targetScript + "|" + methodName + "|" + line;
                    if (emitted.Add(key))
                    {
                        yield return DiscoveredCall.Cross(targetScript, methodName, cleaned.Trim(), line, FlowEventKind.CrossScriptCall);
                    }
                }

                foreach (Match match in MemberCallRegex.Matches(cleaned))
                {
                    if (IntersectsAny(match.Index, match.Length, occupiedRanges))
                    {
                        continue;
                    }

                    var expression = NormalizeExpression(match.Groups["expression"].Value);
                    var methodName = match.Groups["method"].Value;
                    if (ShouldIgnoreCall(methodName))
                    {
                        continue;
                    }

                    var targetScript = ResolveTargetScript(expression, method.ClassName, context);
                    if (string.IsNullOrEmpty(targetScript) || !CanResolve(context, targetScript, methodName))
                    {
                        continue;
                    }

                    occupiedRanges.Add(new RangeSpan(match.Index, match.Length));
                    var key = "member|" + targetScript + "|" + methodName + "|" + line;
                    if (emitted.Add(key))
                    {
                        yield return DiscoveredCall.Cross(targetScript, methodName, cleaned.Trim(), line, FlowEventKind.CrossScriptCall);
                    }
                }

                foreach (Match match in EventSubscriptionRegex.Matches(cleaned))
                {
                    var methodName = match.Groups["method"].Value;
                    if (!localMethods.Contains(methodName))
                    {
                        continue;
                    }

                    var key = "event|" + methodName + "|" + line;
                    if (emitted.Add(key))
                    {
                        yield return DiscoveredCall.Same(methodName, cleaned.Trim(), line, FlowEventKind.EventSubscriber);
                    }
                }

                foreach (Match match in DirectCallRegex.Matches(cleaned))
                {
                    var methodName = match.Groups["method"].Value;
                    if (ShouldIgnoreCall(methodName) || methodName == method.MethodName || !localMethods.Contains(methodName) || IsPartOfMemberCall(cleaned, match.Index, match.Length, occupiedRanges))
                    {
                        continue;
                    }

                    var key = "same|" + methodName + "|" + line;
                    if (emitted.Add(key))
                    {
                        yield return DiscoveredCall.Same(methodName, cleaned.Trim(), line, FlowEventKind.MethodEntry);
                    }
                }
            }
        }

        private static void ApplyRecordedEvents(FlowGraph graph, IReadOnlyList<FlowRecordEvent> recordedEvents, BuildContext context)
        {
            foreach (var recordEvent in recordedEvents)
            {
                var method = ResolveMethod(context.MethodsByClass, recordEvent.ScriptName, recordEvent.MethodName);
                var node = method == null
                    ? GetOrCreateNode(graph, recordEvent.ScriptName, recordEvent.MethodName, AssetPathFor(context, recordEvent.ScriptName), 1, IsUnityMessage(recordEvent.MethodName))
                    : GetOrCreateNode(graph, method, context);
                node.HitCount++;

                if (!string.IsNullOrEmpty(recordEvent.TargetScriptName))
                {
                    var target = GetOrCreateNode(graph, recordEvent.TargetScriptName, recordEvent.TargetMethodName, AssetPathFor(context, recordEvent.TargetScriptName), 1, IsUnityMessage(recordEvent.TargetMethodName));
                    AddEdge(graph, node.Id, target.Id, recordEvent.Kind, FlowRelationshipKind.CrossScript, string.Empty, node.AssetPath, node.Line).HitCount++;
                }
            }
        }

        private static string ResolveTargetScript(string expression, string declaringClass, BuildContext context)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return string.Empty;
            }

            var parts = expression.Split('.');
            if (parts.Length == 0)
            {
                return string.Empty;
            }

            var first = parts[0];
            if (context.MethodsByClass.ContainsKey(first))
            {
                return first;
            }

            if (context.FieldsByClass.TryGetValue(declaringClass, out var fields))
            {
                var field = fields.FirstOrDefault(candidate => candidate.Name == first);
                if (field != null)
                {
                    var fieldType = CleanTypeName(field.TypeName);
                    return context.MethodsByClass.ContainsKey(fieldType) ? fieldType : string.Empty;
                }
            }

            return string.Empty;
        }

        private static AnalyzedMethodInfo ResolveMethod(IReadOnlyDictionary<string, List<AnalyzedMethodInfo>> methodsByClass, string className, string methodName)
        {
            return methodsByClass.TryGetValue(className, out var methods)
                ? methods.FirstOrDefault(method => method.MethodName == methodName)
                : null;
        }

        private static bool CanResolve(BuildContext context, string className, string methodName)
        {
            return ResolveMethod(context.MethodsByClass, className, methodName) != null;
        }

        private static FlowGraphNode GetOrCreateNode(FlowGraph graph, AnalyzedMethodInfo method, BuildContext context)
        {
            return GetOrCreateNode(graph, method.ClassName, method.MethodName, AssetPathFor(context, method.ClassName), method.StartLine, IsUnityMessage(method.MethodName));
        }

        private static FlowGraphNode GetOrCreateNode(FlowGraph graph, string scriptName, string methodName, string assetPath, int line, bool isUnityMessage)
        {
            var id = scriptName + "." + methodName;
            var node = graph.Nodes.FirstOrDefault(candidate => candidate.Id == id);
            if (node != null)
            {
                return node;
            }

            node = new FlowGraphNode(scriptName, methodName, assetPath, line, isUnityMessage);
            graph.Nodes.Add(node);
            return node;
        }

        private static FlowGraphEdge AddEdge(FlowGraph graph, string fromId, string toId, FlowEventKind kind, FlowRelationshipKind relationshipKind, string evidence, string assetPath, int line)
        {
            var edge = graph.Edges.FirstOrDefault(candidate => candidate.FromId == fromId && candidate.ToId == toId && candidate.RelationshipKind == relationshipKind && candidate.Kind == kind);
            if (edge != null)
            {
                return edge;
            }

            edge = new FlowGraphEdge(fromId, toId, kind, relationshipKind, evidence, assetPath, line);
            graph.Edges.Add(edge);
            return edge;
        }

        private static bool LooksLikeDeclaration(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("public ") ||
                   trimmed.StartsWith("private ") ||
                   trimmed.StartsWith("protected ") ||
                   trimmed.StartsWith("internal ") ||
                   trimmed.StartsWith("static ") ||
                   trimmed.StartsWith("void ") ||
                   trimmed.StartsWith("IEnumerator ") ||
                   trimmed.StartsWith("async ");
        }

        private static bool ShouldIgnoreCall(string methodName)
        {
            return string.IsNullOrEmpty(methodName) || IgnoredCalls.Contains(methodName);
        }

        private static bool IsPartOfMemberCall(string line, int methodIndex, int length, IReadOnlyList<RangeSpan> occupiedRanges)
        {
            if (IntersectsAny(methodIndex, length, occupiedRanges))
            {
                return true;
            }

            var prefix = methodIndex > 0 ? line.Substring(0, methodIndex) : string.Empty;
            return prefix.TrimEnd().EndsWith(".");
        }

        private static bool IntersectsAny(int index, int length, IReadOnlyList<RangeSpan> ranges)
        {
            var end = index + length;
            foreach (var range in ranges)
            {
                if (index < range.End && end > range.Start)
                {
                    return true;
                }
            }

            return false;
        }

        private static string StripCommentsAndStrings(string line, ref bool blockComment)
        {
            var builder = new StringBuilder(line.Length);
            var inString = false;
            var inChar = false;
            var verbatimString = false;

            for (var i = 0; i < line.Length; i++)
            {
                var current = line[i];
                var next = i + 1 < line.Length ? line[i + 1] : '\0';

                if (blockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        blockComment = false;
                        i++;
                    }

                    builder.Append(' ');
                    continue;
                }

                if (!inString && !inChar && current == '/' && next == '*')
                {
                    blockComment = true;
                    builder.Append(' ');
                    i++;
                    continue;
                }

                if (!inString && !inChar && current == '/' && next == '/')
                {
                    break;
                }

                if (!inChar && current == '"')
                {
                    if (!inString && i > 0 && line[i - 1] == '@')
                    {
                        verbatimString = true;
                    }
                    else if (inString && verbatimString && next == '"')
                    {
                        i++;
                    }
                    else if (!IsEscaped(line, i) || verbatimString)
                    {
                        inString = !inString;
                        if (!inString)
                        {
                            verbatimString = false;
                        }
                    }

                    builder.Append(' ');
                    continue;
                }

                if (!inString && current == '\'')
                {
                    if (!inChar || !IsEscaped(line, i))
                    {
                        inChar = !inChar;
                    }

                    builder.Append(' ');
                    continue;
                }

                builder.Append(inString || inChar ? ' ' : current);
            }

            return builder.ToString();
        }

        private static bool IsEscaped(string line, int index)
        {
            var slashCount = 0;
            for (var i = index - 1; i >= 0 && line[i] == '\\'; i--)
            {
                slashCount++;
            }

            return slashCount % 2 == 1;
        }

        private static string NormalizeExpression(string expression)
        {
            return Regex.Replace(expression, @"\s+", string.Empty);
        }

        private static bool IsUnityMessage(string methodName)
        {
            return UnityMessageOrder(methodName) < 99;
        }

        private static int UnityMessageOrder(string methodName)
        {
            switch (methodName)
            {
                case "Awake":
                    return 0;
                case "OnEnable":
                    return 1;
                case "Start":
                    return 2;
                case "Update":
                    return 3;
                case "FixedUpdate":
                    return 4;
                case "LateUpdate":
                    return 5;
                case "OnDisable":
                    return 6;
                case "OnDestroy":
                    return 7;
                default:
                    return 99;
            }
        }

        private static string CleanTypeName(string typeName)
        {
            var clean = typeName.Replace("[]", string.Empty).Trim();
            var genericStart = clean.IndexOf('<');
            if (genericStart < 0)
            {
                return clean;
            }

            var genericEnd = clean.LastIndexOf('>');
            return genericEnd > genericStart ? clean.Substring(genericStart + 1, genericEnd - genericStart - 1).Split(',')[0].Trim() : clean;
        }

        private static string AssetPathFor(BuildContext context, string className)
        {
            return context.ScriptPaths.TryGetValue(className, out var assetPath) ? assetPath : string.Empty;
        }

        private sealed class BuildContext
        {
            public BuildContext(
                IReadOnlyDictionary<string, List<AnalyzedMethodInfo>> methodsByClass,
                IReadOnlyDictionary<string, List<ScriptFieldNode>> fieldsByClass,
                IReadOnlyDictionary<string, string> scriptPaths)
            {
                MethodsByClass = methodsByClass;
                FieldsByClass = fieldsByClass;
                ScriptPaths = scriptPaths;
            }

            public IReadOnlyDictionary<string, List<AnalyzedMethodInfo>> MethodsByClass { get; }
            public IReadOnlyDictionary<string, List<ScriptFieldNode>> FieldsByClass { get; }
            public IReadOnlyDictionary<string, string> ScriptPaths { get; }
        }

        private readonly struct RangeSpan
        {
            public RangeSpan(int start, int length)
            {
                Start = start;
                End = start + length;
            }

            public int Start { get; }
            public int End { get; }
        }

        private sealed class DiscoveredCall
        {
            public string TargetScript { get; }
            public string MethodName { get; }
            public string Evidence { get; }
            public int Line { get; }
            public FlowRelationshipKind RelationshipKind { get; }
            public FlowEventKind EventKind { get; }

            private DiscoveredCall(string targetScript, string methodName, string evidence, int line, FlowRelationshipKind relationshipKind, FlowEventKind eventKind)
            {
                TargetScript = targetScript;
                MethodName = methodName;
                Evidence = evidence;
                Line = line;
                RelationshipKind = relationshipKind;
                EventKind = eventKind;
            }

            public static DiscoveredCall Same(string methodName, string evidence, int line, FlowEventKind eventKind)
            {
                return new DiscoveredCall(string.Empty, methodName, evidence, line, FlowRelationshipKind.SameScript, eventKind);
            }

            public static DiscoveredCall Cross(string targetScript, string methodName, string evidence, int line, FlowEventKind eventKind)
            {
                return new DiscoveredCall(targetScript, methodName, evidence, line, FlowRelationshipKind.CrossScript, eventKind);
            }
        }
    }
}
