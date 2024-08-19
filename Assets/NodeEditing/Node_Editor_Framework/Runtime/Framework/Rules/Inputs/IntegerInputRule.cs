using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Inputs
{
    [RuleMenu(Path = "Input/Integer")]
    [RuleTitle(Title = "Int")]
    public class IntegerInputRule : Rule<int>
    {
        public IntegerInputRule()
        {
            RuleName = "Hal.IntIn";
            RuleType = RuleType.Processor;
            Logic = () => { return lastValue = Value; };
        }

        public sealed override Func<int> Logic { get; set; }
        public int Value { get; set; }

        public override void DrawUI()
        {
            Value = RTEditorGUI.IntField(new GUIContent("Value", "The input value of type integer"), Value, MarkCircuitAsDirty);
        }


        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Value = reader.ReadInt32();
        }
    }
}