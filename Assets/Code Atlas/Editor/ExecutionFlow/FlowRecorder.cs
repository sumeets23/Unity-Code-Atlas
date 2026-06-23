using System.Collections.Generic;
using ScriptIntelligence.Editor.Analysis;
using UnityEditor;
using UnityEngine;

namespace ScriptIntelligence.Editor.ExecutionFlow
{
    public sealed class FlowRecorder
    {
        private readonly List<FlowRecordEvent> events = new List<FlowRecordEvent>();
        private IReadOnlyList<ParsedMethod> staticMethods = new List<ParsedMethod>();
        private string selectedScript;
        private bool recording;
        private int lastFrame = -1;

        public bool IsRecording => recording;
        public IReadOnlyList<FlowRecordEvent> Events => events;

        public void Configure(string scriptName, IReadOnlyList<ParsedMethod> methods)
        {
            selectedScript = scriptName;
            staticMethods = methods ?? new List<ParsedMethod>();
        }

        public void Start()
        {
            events.Clear();
            recording = true;
            lastFrame = -1;
            EditorApplication.update += CaptureUnityMessageSamples;
        }

        public void Stop()
        {
            EditorApplication.update -= CaptureUnityMessageSamples;
            recording = false;
        }

        private void CaptureUnityMessageSamples()
        {
            if (!EditorApplication.isPlaying || string.IsNullOrEmpty(selectedScript) || Time.frameCount == lastFrame)
            {
                return;
            }

            lastFrame = Time.frameCount;
            foreach (var method in staticMethods)
            {
                if (method.ClassName != selectedScript || !IsUnityMessage(method.MethodName))
                {
                    continue;
                }

                events.Add(new FlowRecordEvent(FlowEventKind.UnityMessage, method.ClassName, method.MethodName, string.Empty, string.Empty, Time.frameCount, EditorApplication.timeSinceStartup));
            }
        }

        private static bool IsUnityMessage(string methodName)
        {
            return methodName == "Update" || methodName == "FixedUpdate" || methodName == "LateUpdate" || methodName == "Start" || methodName == "Awake" || methodName == "OnEnable" || methodName == "OnDisable";
        }
    }
}
