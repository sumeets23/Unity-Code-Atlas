using ScriptIntelligence.Editor.Models;
using UnityEngine;

namespace ScriptIntelligence.Editor.UIToolkit.Theme
{
    public static class StudioThemeManager
    {
        public static readonly Color Background = FromHex("#0B1020");
        public static readonly Color Panel = FromHex("#111827");
        public static readonly Color Card = FromHex("#0F172A");
        public static readonly Color Border = FromHex("#1F2937");
        public static readonly Color TextPrimary = FromHex("#E5E7EB");
        public static readonly Color TextSecondary = FromHex("#94A3B8");
        public static readonly Color Healthy = FromHex("#3B82F6");
        public static readonly Color Warning = FromHex("#F59E0B");
        public static readonly Color Critical = FromHex("#EF4444");
        public static readonly Color Isolated = FromHex("#64748B");
        public static readonly Color MethodCall = FromHex("#22C55E");
        public static readonly Color Event = FromHex("#F97316");
        public static readonly Color Singleton = FromHex("#A855F7");
        public static readonly Color Inheritance = FromHex("#EAB308");
        public static readonly Color Interface = FromHex("#06B6D4");

        public static Color Severity(AnalysisSeverity severity)
        {
            switch (severity)
            {
                case AnalysisSeverity.Critical:
                    return Critical;
                case AnalysisSeverity.Warning:
                    return Warning;
                default:
                    return Healthy;
            }
        }

        public static Color Relationship(ScriptRelationshipType relationshipType)
        {
            switch (relationshipType)
            {
                case ScriptRelationshipType.MethodCall:
                    return MethodCall;
                case ScriptRelationshipType.EventSubscription:
                    return Event;
                case ScriptRelationshipType.SingletonUsage:
                    return Singleton;
                case ScriptRelationshipType.Inheritance:
                    return Inheritance;
                case ScriptRelationshipType.InterfaceImplementation:
                    return Interface;
                default:
                    return Healthy;
            }
        }

        private static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
