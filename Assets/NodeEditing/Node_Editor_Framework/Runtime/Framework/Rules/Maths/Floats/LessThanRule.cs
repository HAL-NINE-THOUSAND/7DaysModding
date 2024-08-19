using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Float/Less Than", Hidden = true)]
    [RuleTitle(Title = "<")]
    public class FloatLessThanRule : TargetRule<bool, float>
    {
        public const string Identifier = "Hal.Float.LT";

        public FloatLessThanRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value, out _);
                SetLastValue(value < Target);
                return lastValue;
            };
            Input1 = Port<float>.Create("<", this);


            ConvertTypes.Add(typeof(int), LessThanRule.Identifier);
        }

        public Port<float> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Target);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Target = reader.ReadSingle();
        }

        public override void DrawUI()
        {
            Target = RTEditorGUI.FloatField(new GUIContent("Value", "Target Value"), Target, MarkCircuitAsDirty);
        }
    }
}