using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Analysis;
using ScriptIntelligence.Editor.ExecutionFlow;
using ScriptIntelligence.Editor.Models;
using ScriptIntelligence.Editor.UIToolkit.ExecutionFlow;
using ScriptIntelligence.Editor.UIToolkit.Galaxy;
using ScriptIntelligence.Editor.UIToolkit.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptIntelligence.Editor.UIToolkit
{
    public sealed class ScriptIntelligenceStudioWindow : EditorWindow
    {
        private const string UxmlPath = "Assets/ScriptIntelligence/Editor/UIToolkit/UXML/ScriptIntelligenceStudioWindow.uxml";
        private const string UssPath = "Assets/ScriptIntelligence/Editor/UIToolkit/USS/ScriptIntelligenceStudio.uss";

        private readonly ScriptAnalysisPipeline pipeline = new ScriptAnalysisPipeline();
        private readonly FlowRecorder flowRecorder = new FlowRecorder();
        private readonly FlowGraphBuilder flowGraphBuilder = new FlowGraphBuilder();
        private ScriptIntelligenceReport report = new ScriptIntelligenceReport();
        private ProjectExplorerView projectExplorer;
        private DependencyGalaxyView architectureGalaxy;
        private ExecutionFlowView executionFlowView;
        private ExplorerInspectorView inspector;
        private VisualElement centerPanel;
        private TextField globalSearch;
        private Button architectureButton;
        private Button executionButton;
        private Button recordButton;
        private string selectedScript;
        private string analyzedFlowScript;
        private bool flowAnalysisComplete;
        private ExplorerModule activeModule = ExplorerModule.CodeMap;

        [MenuItem("Tools/Unity Code Atlas/Open")]
        public static void Open()
        {
            var window = GetWindow<ScriptIntelligenceStudioWindow>();
            window.titleContent = new GUIContent("Unity Code Atlas");
            window.minSize = new Vector2(1180, 720);
            window.Show();
        }

        private void OnEnable()
        {
            BuildUi();
            EditorApplication.update += Tick;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            flowRecorder.Stop();
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree != null)
            {
                visualTree.CloneTree(rootVisualElement);
            }
            else
            {
                BuildFallbackLayout();
            }

            rootVisualElement.Q<Button>("scan-button").clicked += ScanScene;
            architectureButton = rootVisualElement.Q<Button>("architecture-module-button");
            executionButton = rootVisualElement.Q<Button>("execution-module-button");
            recordButton = rootVisualElement.Q<Button>("record-flow-button");
            globalSearch = rootVisualElement.Q<TextField>("global-search");
            globalSearch.RegisterValueChangedCallback(evt => projectExplorer?.SetSearch(evt.newValue));
            architectureButton.clicked += () => SetModule(ExplorerModule.CodeMap);
            executionButton.clicked += () => SetModule(ExplorerModule.ExecutionFlow);
            recordButton.clicked += ToggleFlowAnalysis;

            projectExplorer = new ProjectExplorerView(rootVisualElement.Q<VisualElement>("left-panel"));
            projectExplorer.ScriptSelected += SelectScript;
            inspector = new ExplorerInspectorView(rootVisualElement.Q<VisualElement>("right-panel"));
            centerPanel = rootVisualElement.Q<VisualElement>("center-panel");
            RefreshAll();
        }

        private void ScanScene()
        {
            report = pipeline.ScanScene();
            selectedScript = report.ScriptGraphNodes.Select(node => node.ClassName).OrderBy(name => name).FirstOrDefault();
            ConfigureRecorder();
            RefreshAll();
        }

        private void SelectScript(string scriptName)
        {
            selectedScript = scriptName;
            if (analyzedFlowScript != selectedScript)
            {
                flowAnalysisComplete = false;
                if (flowRecorder.IsRecording)
                {
                    flowRecorder.Stop();
                }
            }

            ConfigureRecorder();
            RefreshAll();
        }

        private void SetModule(ExplorerModule module)
        {
            activeModule = module;
            if (activeModule == ExplorerModule.ExecutionFlow && !IsSelectedMonoBehaviour())
            {
                selectedScript = string.Empty;
            }

            RefreshAll();
        }

        private void ToggleFlowAnalysis()
        {
            AnalyzeStaticFlow();
            RefreshAll();
        }

        private void AnalyzeStaticFlow()
        {
            if (!IsSelectedMonoBehaviour())
            {
                activeModule = ExplorerModule.ExecutionFlow;
                return;
            }

            if (flowRecorder.IsRecording)
            {
                flowRecorder.Stop();
            }

            analyzedFlowScript = selectedScript;
            flowAnalysisComplete = true;
            ConfigureRecorder();
            activeModule = ExplorerModule.ExecutionFlow;
        }

        private void ConfigureRecorder()
        {
            flowRecorder.Configure(selectedScript, BuildParsedMethods());
        }

        private List<ParsedMethod> BuildParsedMethods()
        {
            return report.AnalyzedMethods
                .Select(method => new ParsedMethod(method.ClassName, method.MethodName, "private", method.Body, method.StartLine, method.EndLine))
                .ToList();
        }

        private void RefreshAll()
        {
            architectureButton?.EnableInClassList("selected", activeModule == ExplorerModule.CodeMap);
            executionButton?.EnableInClassList("selected", activeModule == ExplorerModule.ExecutionFlow);
            recordButton.style.display = activeModule == ExplorerModule.ExecutionFlow ? DisplayStyle.Flex : DisplayStyle.None;
            recordButton.text = FlowActionLabel();
            projectExplorer?.Bind(report, selectedScript);
            RenderCenter();
            RenderInspector();
        }

        private void RenderCenter()
        {
            if (centerPanel == null)
            {
                return;
            }

            centerPanel.Clear();
            if (activeModule == ExplorerModule.CodeMap)
            {
                architectureGalaxy = new DependencyGalaxyView();
                architectureGalaxy.NodeSelected += SelectScript;
                architectureGalaxy.SetData(report, selectedScript);
                centerPanel.Add(architectureGalaxy);
                centerPanel.Add(CanvasHeader("Code Map", "Scripts and relationships discovered in the open scene."));
            }
            else
            {
                if (!IsSelectedMonoBehaviour())
                {
                    centerPanel.Add(BuildMonoBehaviourPicker());
                }
                else
                {
                    centerPanel.Add(BuildExecutionFlowSurface());
                }
            }
        }

        private void RenderInspector()
        {
            if (activeModule == ExplorerModule.CodeMap)
            {
                inspector?.BindArchitecture(report, selectedScript);
            }
            else
            {
                if (IsSelectedMonoBehaviour())
                {
                    inspector?.BindFlow(ShouldShowFlowGraph() ? BuildCurrentFlowGraph() : new FlowGraph(), selectedScript);
                }
                else
                {
                    inspector?.BindArchitecture(report, string.Empty);
                }
            }
        }

        private FlowGraph BuildCurrentFlowGraph()
        {
            return flowGraphBuilder.Build(selectedScript, report, new List<FlowRecordEvent>());
        }

        private bool ShouldShowFlowGraph()
        {
            return flowAnalysisComplete && analyzedFlowScript == selectedScript;
        }

        private string FlowActionLabel()
        {
            return "Analyze Flow";
        }

        private VisualElement BuildExecutionFlowSurface()
        {
            var surface = new VisualElement();
            surface.style.flexGrow = 1f;

            if (!ShouldShowFlowGraph())
            {
                surface.Add(BuildFlowReadyState());
                return surface;
            }

            var graph = BuildCurrentFlowGraph();
            if (graph.Nodes.Count == 0)
            {
                surface.Add(BuildFlowEmptyResult());
                return surface;
            }

            executionFlowView = new ExecutionFlowView();
            executionFlowView.Bind(graph);
            surface.Add(executionFlowView);
            return surface;
        }

        private VisualElement BuildFlowReadyState()
        {
            var panel = new VisualElement();
            panel.AddToClassList("flow-empty-state");
            panel.Add(Label(selectedScript, "product-title"));
            panel.Add(Label("Click Analyze Flow to build a source-backed flowchart from available Unity lifecycle methods and confirmed method calls.", "panel-subtitle"));
            var button = new Button(ToggleFlowAnalysis) { text = "Analyze Flow" };
            button.AddToClassList("primary-button");
            button.style.marginTop = 18f;
            button.style.marginLeft = 0f;
            panel.Add(button);
            return panel;
        }

        private VisualElement BuildFlowEmptyResult()
        {
            var panel = new VisualElement();
            panel.AddToClassList("flow-empty-state");
            panel.Add(Label(selectedScript, "product-title"));
            panel.Add(Label("No Unity lifecycle methods or confirmed method calls were discovered for this script.", "panel-subtitle"));
            var button = new Button(ToggleFlowAnalysis) { text = "Analyze Again" };
            button.AddToClassList("primary-button");
            button.style.marginTop = 18f;
            button.style.marginLeft = 0f;
            panel.Add(button);
            return panel;
        }

        private bool IsSelectedMonoBehaviour()
        {
            if (string.IsNullOrEmpty(selectedScript))
            {
                return false;
            }

            return report.SceneScriptNodes.Any(node => node.TypeName == selectedScript) ||
                   report.ScriptGraphNodes.Any(node => node.ClassName == selectedScript && node.Methods.Count > 0) ||
                   report.AnalyzedMethods.Any(method => method.ClassName == selectedScript);
        }

        private VisualElement BuildMonoBehaviourPicker()
        {
            var picker = new VisualElement();
            picker.style.flexGrow = 1f;
            picker.style.paddingLeft = 32f;
            picker.style.paddingRight = 32f;
            picker.style.paddingTop = 86f;
            picker.Add(Label("Select a Runtime Script", "product-title"));
            picker.Add(Label("Choose the runtime script that should anchor the flowchart.", "panel-subtitle"));

            var scroll = new ScrollView();
            scroll.style.marginTop = 22f;
            scroll.style.flexGrow = 1f;
            picker.Add(scroll);

            var sceneScriptNames = report.SceneScriptNodes.Select(node => node.TypeName).ToHashSet();
            foreach (var node in report.ScriptGraphNodes.Where(node => sceneScriptNames.Contains(node.ClassName) || node.Methods.Count > 0).OrderBy(node => node.ClassName))
            {
                var captured = node.ClassName;
                var incoming = report.ScriptRelationshipEdges.Count(edge => edge.ToType == captured);
                var outgoing = report.ScriptRelationshipEdges.Count(edge => edge.FromType == captured);
                var card = new Button(() =>
                {
                    selectedScript = captured;
                    ConfigureRecorder();
                    RefreshAll();
                });
                card.AddToClassList("script-card");
                card.style.height = 76f;
                card.Add(Label(captured, "script-title"));
                card.Add(Label("Incoming " + incoming + "  /  Outgoing " + outgoing, "meta-text"));
                scroll.Add(card);
            }

            if (report.ScriptGraphNodes.All(node => !sceneScriptNames.Contains(node.ClassName) && node.Methods.Count == 0))
            {
                picker.Add(Label("No runtime scripts were discovered in the open scene.", "panel-subtitle"));
            }

            return picker;
        }

        private void Tick()
        {
            if (!flowRecorder.IsRecording)
            {
                return;
            }

            recordButton.text = FlowActionLabel();
            if (activeModule == ExplorerModule.ExecutionFlow)
            {
                if (ShouldShowFlowGraph())
                {
                    executionFlowView?.Bind(BuildCurrentFlowGraph());
                    inspector?.BindFlow(BuildCurrentFlowGraph(), selectedScript);
                }
            }
        }

        private static VisualElement CanvasHeader(string title, string subtitle)
        {
            var header = new VisualElement();
            header.AddToClassList("canvas-header");
            var titleBlock = new VisualElement();
            titleBlock.Add(Label(title, "panel-title"));
            titleBlock.Add(Label(subtitle, "panel-subtitle"));
            header.Add(titleBlock);
            return header;
        }

        private static Label Label(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        private void BuildFallbackLayout()
        {
            var root = new VisualElement { name = "studio-root" };
            root.AddToClassList("studio-root");
            rootVisualElement.Add(root);
        }

        private enum ExplorerModule
        {
            CodeMap,
            ExecutionFlow
        }
    }
}

