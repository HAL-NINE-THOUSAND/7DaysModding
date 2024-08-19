using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Inputs
{
    [RuleMenu(Path = "Input/Long")]
    public class LongInputRule : Rule<long>
    {
        public LongInputRule()
        {
            RuleName = "Hal.LongIn";
            RuleType = RuleType.Processor;
            Logic = () => { return lastValue = Value; };
        }

        public sealed override Func<long> Logic { get; set; }
        public long Value { get; set; }

        public override void DrawUI()
        {
            //will this work? 32 bit -> 64?
            Value = (long)RTEditorGUI.FloatField(new GUIContent("Value", "The input value of type integer"), Value, MarkCircuitAsDirty);
        }


        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Value = reader.ReadInt64();
        }
    }
}