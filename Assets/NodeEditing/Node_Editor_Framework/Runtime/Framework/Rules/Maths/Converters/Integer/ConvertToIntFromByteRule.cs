using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Convert/Byte To Integer", Hidden = true)]
    [RuleTitle(Title = "byte to int")]
    public class ConvertToIntFromByteRule : Rule<int>
    {
        public const string Identifier = "Hal.Byte2Int";

        public ConvertToIntFromByteRule()
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
            Input1 = Port<byte>.Create("Input 1", this);

            ConvertTypes.Add(typeof(short), ConvertToIntFromShortRule.Identifier);
            ConvertTypes.Add(typeof(float), ConvertToIntFromFloatRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToIntFromDoubleRule.Identifier);
        }

        public Port<byte> Input1 { get; set; }
        public sealed override Func<int> Logic { get; set; }
    }
}