using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Float/Not Equal", Hidden = true)]
    [RuleTitle(Title = "!=")]
    public class FloatNotEqualRule : TargetRule<bool, float>
    {
        public const string Identifier = "Hal.Float.NE";

        public FloatNotEqualRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value);
                SetLastValue(value != Target);
                return lastValue;
            };
            Input1 = Port<float>.Create("!=", this);

            ConvertTypes.Add(typeof(int), NotEqualRule.Identifier);
        }

        public Port<float> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }

        public override void CopyDetailsFromRule(IRule rule)
        {
            base.CopyDetailsFromRule(rule);
        }

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