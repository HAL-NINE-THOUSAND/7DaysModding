using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Inputs
{
    [RuleMenu(Path = "Input/Double")]
    [RuleTitle(Title = "Double")]
    public class DoubleInputRule : Rule<double>
    {
        public DoubleInputRule()
        {
            RuleName = "Hal.DoubleIn";
            RuleType = RuleType.Processor;
            Logic = () => { return lastValue = Value; };
        }

        public sealed override Func<double> Logic { get; set; }
        public double Value { get; set; }

        public override void DrawUI()
        {
            Value = RTEditorGUI.FloatField(new GUIContent("Value", "The input value of type integer"), (float)Value, MarkCircuitAsDirty);
        }


        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Value = reader.ReadDouble();
        }
    }
}