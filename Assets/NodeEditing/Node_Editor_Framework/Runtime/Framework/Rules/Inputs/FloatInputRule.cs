using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Inputs
{
    [RuleMenu(Path = "Input/Float")]
    [RuleTitle(Title = "Float")]
    public class FloatInputRule : Rule<float>
    {
        public FloatInputRule()
        {
            RuleName = "Hal.FloatIn";
            RuleType = RuleType.Processor;
            Logic = () => { return lastValue = Value; };
        }

        public sealed override Func<float> Logic { get; set; }
        public float Value { get; set; }

        public override void DrawUI()
        {
            Value = RTEditorGUI.FloatField(new GUIContent("Value", "The input value of type integer"), Value, MarkCircuitAsDirty);
        }


        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Value = reader.ReadSingle();
        }
    }
}