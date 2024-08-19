using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Maths.Converters.Float
{
    [RuleMenu(Path = "Maths/Convert/Byte To Float", Hidden = true)]
    [RuleTitle(Title = "byte to float")]
    public class ConvertToFloatFromByteRule : Rule<float>
    {
        public const string Identifier = "Hal.Byte2Float";

        public ConvertToFloatFromByteRule()
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

            ConvertTypes.Add(typeof(short), ConvertToFloatFromShortRule.Identifier);
            ConvertTypes.Add(typeof(int), ConvertToFloatFromIntRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToFloatFromDoubleRule.Identifier);
        }

        public Port<byte> Input1 { get; set; }
        public sealed override Func<float> Logic { get; set; }
    }
}