using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.ExecutionFlow;
using ScriptIntelligence.Editor.UIToolkit.Theme;
using ScriptIntelligence.Editor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptIntelligence.Editor.UIToolkit.ExecutionFlow
{
    public sealed class ExecutionFlowView : VisualElement
    {
        private const float NodeWidth = 230f;
        private const float NodeHeight = 64f;
        private const float MinZoom = 0.45f;
        private const float MaxZoom = 1.85f;
        private const float BranchSpacing = 290f;
        private const float RowSpacing = 118f;
        private const float StartX = 72f;
        private const float StartY = 96f;
        private const float LaneLabelY = 46f;

        private readonly Dictionary<string, VisualElement> nodeElements = new Dictionary<string, VisualElement>();
        private readonly Dictionary<string, Label> laneLabelElements = new Dictionary<string, Label>();
        private FlowGraph graph = new FlowGraph();
        private Button resetViewButton;
        private Label zoomLabel;
        private VisualElement timeline;
        private VisualElement legend;
        private Vector2 pan;
        private Vector2 lastPointerPosition;
        private bool isDragging;
        private int draggingPointerId = -1;
        private float zoom = 1f;

        public ExecutionFlowView()
        {
            AddToClassList("flow-canvas");
            generateVisualContent += DrawFlowEdges;
            RegisterCallback<GeometryChangedEvent>(_ => SyncNodes());
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            BuildLegend();
            BuildTimeline();
        }

        public void Bind(FlowGraph value)
        {
            graph = value ?? new FlowGraph();
            SyncNodes();
            MarkDirtyRepaint();
        }

        private void BuildTimeline()
        {
            timeline = new VisualElement();
            timeline.AddToClassList("timeline-row");
            zoomLabel = new Label("100%");
            zoomLabel.AddToClassList("zoom-label");
            timeline.Add(zoomLabel);
            resetViewButton = new Button(ResetView) { text = "Reset View" };
            resetViewButton.AddToClassList("secondary-button");
            timeline.Add(resetViewButton);
            Add(timeline);
        }

        private void BuildLegend()
        {
            legend = new VisualElement();
            legend.AddToClassList("flow-legend");
            AddLegendItem("Unity", "unity");
            AddLegendItem("Same Script", "same-script");
            AddLegendItem("Cross Script", "cross-script");
            AddLegendItem("Event", "event");
            AddLegendItem("Unresolved", "unresolved");
            Add(legend);
        }

        private void AddLegendItem(string text, string className)
        {
            var item = new VisualElement();
            item.AddToClassList("flow-legend-item");
            var dot = new VisualElement();
            dot.AddToClassList("flow-legend-dot");
            dot.AddToClassList(className);
            item.Add(dot);
            var label = new Label(text);
            label.AddToClassList("meta-text");
            item.Add(label);
            legend.Add(item);
        }

        private void SyncNodes()
        {
            var active = new HashSet<string>();
            var layout = BuildLayout();
            foreach (var node in graph.Nodes)
            {
                active.Add(node.Id);
                if (!nodeElements.TryGetValue(node.Id, out var element))
                {
                    element = CreateNode(node);
                    nodeElements.Add(node.Id, element);
                    Add(element);
                }

                if (!layout.Positions.TryGetValue(node.Id, out var graphPosition))
                {
                    continue;
                }

                var position = ToCanvasPosition(graphPosition);
                element.style.left = position.x;
                element.style.top = position.y;
                element.style.width = NodeWidth * zoom;
                element.style.minHeight = NodeHeight * zoom;
                element.style.paddingLeft = Mathf.Max(7f, 10f * zoom);
                element.style.paddingRight = Mathf.Max(7f, 10f * zoom);
                element.style.paddingTop = Mathf.Max(7f, 10f * zoom);
                element.style.paddingBottom = Mathf.Max(7f, 10f * zoom);
                element.EnableInClassList("unity-message", node.IsUnityMessage);
                element.EnableInClassList("cross-script", HasIncomingCrossScript(node.Id));
                element.EnableInClassList("same-script", !node.IsUnityMessage && !HasIncomingCrossScript(node.Id));
                element.style.borderLeftColor = NodeColor(node);
                element.Q<Label>("flow-title").text = node.MethodName + "()";
                element.Q<Label>("flow-meta").text = node.ScriptName;
            }

            foreach (var stale in nodeElements.Keys.Where(key => !active.Contains(key)).ToList())
            {
                nodeElements[stale].RemoveFromHierarchy();
                nodeElements.Remove(stale);
            }

            SyncLaneLabels(layout);
            zoomLabel.text = Mathf.RoundToInt(zoom * 100f) + "%";
            legend.BringToFront();
            timeline.BringToFront();
        }

        private FlowLayout BuildLayout()
        {
            var layout = new FlowLayout();
            var byId = graph.Nodes.ToDictionary(node => node.Id);
            var children = graph.Edges
                .Where(edge => byId.ContainsKey(edge.FromId) && byId.ContainsKey(edge.ToId))
                .GroupBy(edge => edge.FromId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(edge => edge.Line)
                        .ThenBy(RelationshipOrder)
                        .ThenBy(edge => byId[edge.ToId].Line)
                        .ThenBy(edge => edge.ToId)
                        .ToList());

            var roots = graph.Nodes
                .Where(node => node.IsUnityMessage)
                .OrderBy(node => UnityMessageOrder(node.MethodName))
                .ThenBy(node => node.Line)
                .ToList();

            if (roots.Count == 0 && graph.Nodes.Count > 0)
            {
                roots.Add(graph.Nodes.OrderBy(node => node.Line).First());
            }

            var nextRootBranch = 0;
            foreach (var root in roots)
            {
                layout.Lanes[root.Id] = new Vector2(StartX + nextRootBranch * BranchSpacing, LaneLabelY);
                var state = new LayoutState(nextRootBranch + 1);
                LayoutNode(root.Id, nextRootBranch, 0, state, layout, byId, children, new HashSet<string>());
                nextRootBranch = state.MaxBranchUsed + 2;
            }

            var overflowBranch = nextRootBranch;
            foreach (var node in graph.Nodes.Where(node => !layout.Positions.ContainsKey(node.Id)).OrderBy(node => node.Line))
            {
                layout.Positions[node.Id] = BranchPosition(overflowBranch, 0);
                overflowBranch++;
            }

            return layout;
        }

        private void LayoutNode(
            string nodeId,
            int branchIndex,
            int preferredRow,
            LayoutState state,
            FlowLayout layout,
            IReadOnlyDictionary<string, FlowGraphNode> byId,
            IReadOnlyDictionary<string, List<FlowGraphEdge>> children,
            HashSet<string> path)
        {
            if (layout.Positions.ContainsKey(nodeId) || !path.Add(nodeId))
            {
                return;
            }

            var row = state.ClaimRow(branchIndex, preferredRow);
            layout.Positions[nodeId] = BranchPosition(branchIndex, row);
            state.MaxBranchUsed = Mathf.Max(state.MaxBranchUsed, branchIndex);

            if (children.TryGetValue(nodeId, out var outgoing))
            {
                foreach (var edge in outgoing)
                {
                    var childBranch = edge.RelationshipKind == FlowRelationshipKind.CrossScript
                        ? state.AllocateBranch()
                        : branchIndex;
                    LayoutNode(edge.ToId, childBranch, row + 1, state, layout, byId, children, path);
                }
            }

            path.Remove(nodeId);
        }

        private static Vector2 BranchPosition(int branchIndex, int row)
        {
            return new Vector2(StartX + branchIndex * BranchSpacing, StartY + row * RowSpacing);
        }

        private VisualElement CreateNode(FlowGraphNode node)
        {
            var element = new VisualElement();
            element.AddToClassList("flow-node");
            element.RegisterCallback<ClickEvent>(_ => OpenNode(node));
            var title = new Label { name = "flow-title" };
            title.AddToClassList("script-title");
            var meta = new Label { name = "flow-meta" };
            meta.AddToClassList("meta-text");
            element.Add(title);
            element.Add(meta);
            return element;
        }

        private void DrawFlowEdges(MeshGenerationContext context)
        {
            var layout = BuildLayout();
            var painter = context.painter2D;
            foreach (var edge in graph.Edges)
            {
                if (!layout.Positions.TryGetValue(edge.FromId, out var from) || !layout.Positions.TryGetValue(edge.ToId, out var to))
                {
                    continue;
                }

                var color = ColorFor(edge);
                color.a = 0.78f;
                painter.strokeColor = color;
                painter.lineWidth = Mathf.Max(1f, 1.6f * zoom);
                var start = ToCanvasPosition(from + new Vector2(NodeWidth * 0.5f, NodeHeight));
                var end = ToCanvasPosition(to + new Vector2(NodeWidth * 0.5f, 0f));
                var horizontalOffset = Mathf.Abs(end.x - start.x) > 1f ? Mathf.Abs(end.x - start.x) * 0.28f : 0f;
                var c1 = start + new Vector2(horizontalOffset, 44f * zoom);
                var c2 = end - new Vector2(horizontalOffset, 44f * zoom);
                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(c1, c2, end);
                painter.Stroke();
            }
        }

        private void SyncLaneLabels(FlowLayout layout)
        {
            var active = new HashSet<string>();
            foreach (var lane in layout.Lanes)
            {
                active.Add(lane.Key);
                if (!laneLabelElements.TryGetValue(lane.Key, out var label))
                {
                    label = new Label(MethodNameFromId(lane.Key) + "()");
                    label.AddToClassList("flow-lane-label");
                    laneLabelElements[lane.Key] = label;
                    Add(label);
                }

                var position = ToCanvasPosition(lane.Value);
                label.style.left = position.x;
                label.style.top = position.y;
            }

            foreach (var stale in laneLabelElements.Keys.Where(key => !active.Contains(key)).ToList())
            {
                laneLabelElements[stale].RemoveFromHierarchy();
                laneLabelElements.Remove(stale);
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            var previousZoom = zoom;
            var zoomDelta = evt.delta.y > 0f ? 0.90f : 1.10f;
            zoom = Mathf.Clamp(zoom * zoomDelta, MinZoom, MaxZoom);
            var mouse = evt.localMousePosition;
            var graphPointUnderMouse = (mouse - pan) / previousZoom;
            pan = mouse - graphPointUnderMouse * zoom;
            SyncNodes();
            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if ((evt.button != 0 && evt.button != 2) || IsTimelineTarget(evt.target as VisualElement))
            {
                return;
            }

            isDragging = true;
            draggingPointerId = evt.pointerId;
            lastPointerPosition = (Vector2)evt.position;
            this.CapturePointer(draggingPointerId);
            evt.StopPropagation();
        }

        private bool IsTimelineTarget(VisualElement target)
        {
            var current = target;
            while (current != null)
            {
                if (current == timeline)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || draggingPointerId != evt.pointerId)
            {
                return;
            }

            var currentPosition = (Vector2)evt.position;
            pan += currentPosition - lastPointerPosition;
            lastPointerPosition = currentPosition;
            SyncNodes();
            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!isDragging || draggingPointerId != evt.pointerId)
            {
                return;
            }

            isDragging = false;
            this.ReleasePointer(draggingPointerId);
            draggingPointerId = -1;
            evt.StopPropagation();
        }

        private void ResetView()
        {
            pan = Vector2.zero;
            zoom = 1f;
            SyncNodes();
            MarkDirtyRepaint();
        }

        private Vector2 ToCanvasPosition(Vector2 graphPosition)
        {
            return pan + graphPosition * zoom;
        }

        private bool HasIncomingCrossScript(string nodeId)
        {
            return graph.Edges.Any(edge => edge.ToId == nodeId && edge.RelationshipKind == FlowRelationshipKind.CrossScript);
        }

        private static Color NodeColor(FlowGraphNode node)
        {
            return node.IsUnityMessage ? StudioThemeManager.Healthy : StudioThemeManager.Isolated;
        }

        private static Color ColorFor(FlowGraphEdge edge)
        {
            if (edge.Kind == FlowEventKind.EventSubscriber)
            {
                return new Color(0.96f, 0.62f, 0.04f);
            }

            switch (edge.RelationshipKind)
            {
                case FlowRelationshipKind.SameScript:
                    return StudioThemeManager.MethodCall;
                case FlowRelationshipKind.CrossScript:
                    return StudioThemeManager.Interface;
                case FlowRelationshipKind.Unity:
                    return StudioThemeManager.Healthy;
                default:
                    return StudioThemeManager.Isolated;
            }
        }

        private static int RelationshipOrder(FlowGraphEdge edge)
        {
            if (edge.RelationshipKind == FlowRelationshipKind.CrossScript)
            {
                return 0;
            }

            return edge.Kind == FlowEventKind.EventSubscriber ? 1 : 2;
        }

        private static string MethodNameFromId(string id)
        {
            var dot = id.LastIndexOf('.');
            return dot >= 0 && dot + 1 < id.Length ? id.Substring(dot + 1) : id;
        }

        private static void OpenNode(FlowGraphNode node)
        {
            if (!string.IsNullOrEmpty(node.AssetPath))
            {
                SourceNavigation.Open(node.AssetPath, node.Line);
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
                    return 20;
            }
        }

        private sealed class FlowLayout
        {
            public Dictionary<string, Vector2> Positions { get; } = new Dictionary<string, Vector2>();
            public Dictionary<string, Vector2> Lanes { get; } = new Dictionary<string, Vector2>();
        }

        private sealed class LayoutState
        {
            private readonly Dictionary<int, int> branchRows = new Dictionary<int, int>();

            public LayoutState(int nextBranch)
            {
                NextBranch = nextBranch;
            }

            public int NextBranch { get; private set; }
            public int MaxBranchUsed { get; set; }

            public int AllocateBranch()
            {
                var branch = NextBranch;
                NextBranch++;
                MaxBranchUsed = Mathf.Max(MaxBranchUsed, branch);
                return branch;
            }

            public int ClaimRow(int branchIndex, int preferredRow)
            {
                var currentRow = branchRows.TryGetValue(branchIndex, out var row) ? row : 0;
                var claimedRow = Mathf.Max(currentRow, preferredRow);
                branchRows[branchIndex] = claimedRow + 1;
                return claimedRow;
            }
        }
    }
}



