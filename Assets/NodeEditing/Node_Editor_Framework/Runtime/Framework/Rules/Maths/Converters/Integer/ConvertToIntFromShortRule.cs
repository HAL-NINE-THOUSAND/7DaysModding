using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Convert/Short To Integer", Hidden = true)]
    [RuleTitle(Title = "short to int")]
    public class ConvertToIntFromShortRule : Rule<int>
    {
        public const string Identifier = "Hal.Short2Int";

        public ConvertToIntFromShortRule()
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

            ConvertTypes.Add(typeof(byte), ConvertToIntFromByteRule.Identifier);
            ConvertTypes.Add(typeof(float), ConvertToIntFromFloatRule.Identifier);
            ConvertTypes.Add(typeof(double), ConvertToIntFromDoubleRule.Identifier);
        }

        public Port<short> Input1 { get; set; }
        public sealed override Func<int> Logic { get; set; }
    }
}