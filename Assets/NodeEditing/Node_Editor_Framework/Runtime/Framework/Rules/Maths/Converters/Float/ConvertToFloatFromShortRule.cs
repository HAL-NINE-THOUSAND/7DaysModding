using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Maths.Converters.Float
{
    [RuleMenu(Path = "Maths/Convert/Short To Float", Hidden = true)]
    [RuleTitle(Title = "short to float")]
    public class ConvertToFloatFromShortRule : Rule<float>
    {
        public const string Identifier = "Hal.Short2Float";

        public ConvertToFloatFromShortRule()
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
            Input1 = Port<short>.Create("Input 1", this);

            ConvertTypes.Add(typeof(byte), ConvertToFloatFromByteRule.Identifier);
            ConvertTypes.Add(typeof(int), ConvertToFloatFromIntRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToFloatFromDoubleRule.Identifier);
        }

        public Port<short> Input1 { get; set; }
        public sealed override Func<float> Logic { get; set; }
    }
}