using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Maths.Converters.Float
{
    [RuleMenu(Path = "Maths/Convert/Double To Float", Hidden = true)]
    [RuleTitle(Title = "double to float")]
    public class ConvertToFloatFromDoubleRule : Rule<float>
    {
        public const string Identifier = "Hal.Double2Float";

        public ConvertToFloatFromDoubleRule()
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
            Input1 = Port<double>.Create("Input 1", this);

            ConvertTypes.Add(typeof(byte), ConvertToFloatFromByteRule.Identifier);
            ConvertTypes.Add(typeof(short), ConvertToFloatFromShortRule.Identifier);
            ConvertTypes.Add(typeof(int), ConvertToFloatFromIntRule.Identifier);
        }

        public Port<double> Input1 { get; set; }
        public sealed override Func<float> Logic { get; set; }
    }
}