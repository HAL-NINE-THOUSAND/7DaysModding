using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Multiply")]
    public class MultiplyRule : Rule<int>
    {
        public const string Identifier = "Hal.Times";

        public MultiplyRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value1, out _);
                Circuit.GetValue(Input2, out var value2, out _);

                var value = value1 * value2;
                SetLastValue(value);
                return lastValue;
            };
            Input1 = Port<int>.Create("Input 1", this);
            Input2 = Port<int>.Create("Input 2", this);

            ConvertTypes.Add(typeof(float), FloatMultiplyRule.Identifier);
        }

        public Port<int> Input1 { get; set; }
        public Port<int> Input2 { get; set; }
        public sealed override Func<int> Logic { get; set; }
    }
}