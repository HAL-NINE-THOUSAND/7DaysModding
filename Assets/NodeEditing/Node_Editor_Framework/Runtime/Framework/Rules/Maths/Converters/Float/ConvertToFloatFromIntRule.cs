using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Maths.Converters.Float
{
    [RuleMenu(Path = "Maths/Convert/To Float")]
    [RuleTitle(Title = "integer to float")]
    public class ConvertToFloatFromIntRule : Rule<float>
    {
        public const string Identifier = "Hal.Int2Float";

        public ConvertToFloatFromIntRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value1, out _);
                var value = value1;
                SetLastValue(value);
                return lastValue;
            };
            Input1 = Port<int>.Create("Input 1", this);

            ConvertTypes.Add(typeof(byte), ConvertToFloatFromByteRule.Identifier);
            ConvertTypes.Add(typeof(short), ConvertToFloatFromShortRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToFloatFromDoubleRule.Identifier);
        }

        public Port<int> Input1 { get; set; }
        public sealed override Func<float> Logic { get; set; }
    }
}