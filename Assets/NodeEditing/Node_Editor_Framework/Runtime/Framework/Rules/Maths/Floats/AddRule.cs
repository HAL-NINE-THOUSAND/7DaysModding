using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{
    [RuleMenu(Path = "Maths/Float/Add", Hidden = true)]
    [RuleTitle(Title = "Add")]
    public class FloatAddRule : Rule<float>
    {
        public const string Identifier = "Hal.Float.Add";

        public FloatAddRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value1, out _);
                Circuit.GetValue(Input2, out var value2, out _);

                var value = value1 + value2;
                SetLastValue(value);
                return lastValue;
            };
            Input1 = Port<float>.Create("Input 1", this);
            Input2 = Port<float>.Create("Input 2", this);

            ConvertTypes.Add(typeof(int), AddRule.Identifier);
        }

        public Port<float> Input1 { get; set; }
        public Port<float> Input2 { get; set; }
        public sealed override Func<float> Logic { get; set; }
    }
}