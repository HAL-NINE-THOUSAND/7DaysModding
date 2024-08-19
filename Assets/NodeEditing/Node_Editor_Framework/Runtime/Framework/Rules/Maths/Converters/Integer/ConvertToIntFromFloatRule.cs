using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Convert/To Integer")]
    [RuleTitle(Title = "float to Integer")]
    public class ConvertToIntFromFloatRule : Rule<int>
    {
        public const string Identifier = "Hal.Float2Int";

        public ConvertToIntFromFloatRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value1, out _);
                var value = (int)value1;
                SetLastValue(value);
                return lastValue;
            };
            Input1 = Port<float>.Create("Input 1", this);

            ConvertTypes.Add(typeof(short), ConvertToIntFromShortRule.Identifier);
            ConvertTypes.Add(typeof(byte), ConvertToIntFromByteRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToIntFromDoubleRule.Identifier);
        }

        public Port<float> Input1 { get; set; }
        public sealed override Func<int> Logic { get; set; }
    }
}