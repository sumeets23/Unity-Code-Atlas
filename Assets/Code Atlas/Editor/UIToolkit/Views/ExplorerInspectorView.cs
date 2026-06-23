using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.ExecutionFlow;
using ScriptIntelligence.Editor.Models;
using ScriptIntelligence.Editor.Utilities;
using UnityEngine.UIElements;

namespace ScriptIntelligence.Editor.UIToolkit.Views
{
    public sealed class ExplorerInspectorView
    {
        private readonly VisualElement root;
        private VisualElement contentRoot;

        public ExplorerInspectorView(VisualElement root)
        {
            this.root = root;
            contentRoot = root;
        }

        public void BindArchitecture(ScriptIntelligenceReport report, string selectedScript)
        {
            root.Clear();
            contentRoot = root;
            root.Add(Label("Inspector", "panel-title"));
            if (string.IsNullOrEmpty(selectedScript))
            {
                root.Add(Label("Select a script to see its dependencies, fields, and methods.", "panel-subtitle"));
                return;
            }

            var node = report.ScriptGraphNodes.FirstOrDefault(candidate => candidate.ClassName == selectedScript);
            var incoming = report.ScriptRelationshipEdges.Where(edge => edge.ToType == selectedScript).ToList();
            var outgoing = report.ScriptRelationshipEdges.Where(edge => edge.FromType == selectedScript).ToList();

            root.Add(Label(selectedScript, "product-title"));
            root.Add(Label(node == null ? "Script" : NodeTypeLabel(node.NodeType), "panel-subtitle"));
            AddSummary("Connections", incoming.Count + " incoming  /  " + outgoing.Count + " outgoing");
            AddSection("Incoming", incoming.Select(edge => edge.FromType + "  /  " + RelationshipLabel(edge.RelationshipType)).Take(8).ToArray());
            AddSection("Outgoing", outgoing.Select(edge => edge.ToType + "  /  " + RelationshipLabel(edge.RelationshipType)).Take(8).ToArray());

            if (node == null)
            {
                return;
            }

            AddSection("Fields", node.Fields.Select(field => field.TypeName + " " + field.Name).Take(8).ToArray());
            AddSection("Methods", node.Methods.Select(method => method.Name + "()").Take(10).ToArray());
            var open = new Button(() => SourceNavigation.Open(node.AssetPath, node.Line)) { text = "Open Script" };
            open.AddToClassList("open-script-button");
            root.Add(open);
        }

        public void BindFlow(FlowGraph graph, string selectedScript)
        {
            root.Clear();
            root.Add(Label("Flow Inspector", "panel-title"));
            root.Add(Label(string.IsNullOrEmpty(selectedScript) ? "No script selected" : selectedScript, "product-title"));
            root.Add(Label("Static flow from Unity messages, confirmed method calls, and event subscriptions.", "panel-subtitle"));
            var scroll = new ScrollView();
            scroll.AddToClassList("inspector-scroll");
            root.Add(scroll);
            contentRoot = scroll;
            AddSummary("Graph", graph.Nodes.Count + " methods  /  " + graph.Edges.Count + " paths");
            AddFlowSections(graph);
        }

        private void AddFlowSections(FlowGraph graph)
        {
            var byId = graph.Nodes.ToDictionary(node => node.Id);
            var outgoing = graph.Edges
                .GroupBy(edge => edge.FromId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(FlowSortOrder)
                        .ThenBy(edge => byId.ContainsKey(edge.ToId) ? byId[edge.ToId].Line : edge.Line)
                        .ThenBy(edge => edge.ToId)
                        .ToList());

            var roots = graph.Nodes
                .Where(node => node.IsUnityMessage)
                .OrderBy(node => UnityMessageOrder(node.MethodName))
                .ThenBy(node => node.Line)
                .ToList();

            if (roots.Count == 0)
            {
                AddSection("Flow", graph.Nodes.Select(node => node.ScriptName + "." + node.MethodName + "()").Take(12).ToArray());
                return;
            }

            foreach (var rootNode in roots)
            {
                contentRoot.Add(Label(rootNode.MethodName + "()", "section-title"));
                if (!outgoing.TryGetValue(rootNode.Id, out var rootEdges) || rootEdges.Count == 0)
                {
                    contentRoot.Add(Label("No discovered calls.", "meta-text"));
                    continue;
                }

                var visited = new HashSet<string>();
                foreach (var edge in rootEdges)
                {
                    AddFlowEdgeCard(edge, byId, outgoing, visited, 0);
                }
            }
        }

        private void AddFlowEdgeCard(
            FlowGraphEdge edge,
            IReadOnlyDictionary<string, FlowGraphNode> byId,
            IReadOnlyDictionary<string, List<FlowGraphEdge>> outgoing,
            HashSet<string> visited,
            int depth)
        {
            if (!byId.TryGetValue(edge.FromId, out var from) || !byId.TryGetValue(edge.ToId, out var to))
            {
                return;
            }

            var key = edge.FromId + "->" + edge.ToId + "@" + edge.Line;
            if (!visited.Add(key) || depth > 8)
            {
                return;
            }

            var card = new Button(() => OpenEdge(edge));
            card.AddToClassList("flow-evidence-card");
            card.style.marginLeft = depth * 10f;
            card.Add(Label(from.ScriptName + "." + from.MethodName + "()", "flow-evidence-source"));
            card.Add(Label(FlowAction(edge) + " " + to.ScriptName + "." + to.MethodName + "()", "flow-evidence-target"));
            card.Add(Label("line " + edge.Line + "  /  " + FlowKindLabel(edge), "meta-text"));
            if (!string.IsNullOrEmpty(edge.Evidence))
            {
                card.Add(Label(edge.Evidence, "flow-evidence-code"));
            }

            contentRoot.Add(card);

            if (!outgoing.TryGetValue(edge.ToId, out var childEdges))
            {
                return;
            }

            foreach (var child in childEdges)
            {
                AddFlowEdgeCard(child, byId, outgoing, visited, depth + 1);
            }
        }

        private static void OpenEdge(FlowGraphEdge edge)
        {
            if (!string.IsNullOrEmpty(edge.AssetPath))
            {
                SourceNavigation.Open(edge.AssetPath, edge.Line);
            }
        }

        private static string FlowAction(FlowGraphEdge edge)
        {
            if (edge.Kind == FlowEventKind.EventSubscriber)
            {
                return "subscribes to";
            }

            return edge.RelationshipKind == FlowRelationshipKind.CrossScript ? "calls into" : "calls";
        }

        private static int FlowSortOrder(FlowGraphEdge edge)
        {
            if (edge.RelationshipKind == FlowRelationshipKind.CrossScript)
            {
                return 0;
            }

            return edge.Kind == FlowEventKind.EventSubscriber ? 1 : 2;
        }

        private static string FlowKindLabel(FlowGraphEdge edge)
        {
            if (edge.Kind == FlowEventKind.EventSubscriber)
            {
                return "Event";
            }

            switch (edge.RelationshipKind)
            {
                case FlowRelationshipKind.SameScript:
                    return "Same Script";
                case FlowRelationshipKind.CrossScript:
                    return "Cross Script";
                case FlowRelationshipKind.Unity:
                    return "Unity";
                default:
                    return "Unresolved";
            }
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

        private void AddSummary(string title, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("detail-row");
            row.Add(Label(title, "meta-text"));
            row.Add(Label(value, "script-title"));
            contentRoot.Add(row);
        }

        private void AddSection(string title, string[] rows)
        {
            contentRoot.Add(Label(title, "section-title"));
            if (rows.Length == 0)
            {
                contentRoot.Add(Label("None found.", "meta-text"));
                return;
            }

            foreach (var rowText in rows)
            {
                var row = new Label(rowText);
                row.AddToClassList("detail-row-label");
                contentRoot.Add(row);
            }
        }

        private static string NodeTypeLabel(ScriptNodeType nodeType)
        {
            switch (nodeType)
            {
                case ScriptNodeType.ScriptableObject:
                    return "ScriptableObject";
                case ScriptNodeType.Interface:
                    return "Interface";
                case ScriptNodeType.AbstractClass:
                    return "Abstract class";
                case ScriptNodeType.Manager:
                    return "Manager";
                case ScriptNodeType.Singleton:
                    return "Singleton";
                case ScriptNodeType.MonoBehaviour:
                    return "MonoBehaviour";
                default:
                    return "Class";
            }
        }

        private static string RelationshipLabel(ScriptRelationshipType relationshipType)
        {
            switch (relationshipType)
            {
                case ScriptRelationshipType.SerializedField:
                    return "Serialized field";
                case ScriptRelationshipType.MethodCall:
                    return "Method call";
                case ScriptRelationshipType.EventSubscription:
                    return "Event";
                case ScriptRelationshipType.SingletonUsage:
                    return "Singleton";
                case ScriptRelationshipType.Inheritance:
                    return "Inheritance";
                case ScriptRelationshipType.InterfaceImplementation:
                    return "Interface";
                default:
                    return relationshipType.ToString();
            }
        }

        private static Label Label(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }
    }
}



