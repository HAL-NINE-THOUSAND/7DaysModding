using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Convert/Double To Integer", Hidden = true)]
    [RuleTitle(Title = "double to int")]
    public class ConvertToIntFromDoubleRule : Rule<int>
    {
        public const string Identifier = "Hal.Double2Int";

        public ConvertToIntFromDoubleRule()
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


            ConvertTypes.Add(typeof(short), ConvertToIntFromShortRule.Identifier);
            ConvertTypes.Add(typeof(byte), ConvertToIntFromByteRule.Identifier);
            ConvertTypes.Add(typeof(float), ConvertToIntFromFloatRule.Identifier);
        }

        public Port<double> Input1 { get; set; }
        public sealed override Func<int> Logic { get; set; }
    }
}