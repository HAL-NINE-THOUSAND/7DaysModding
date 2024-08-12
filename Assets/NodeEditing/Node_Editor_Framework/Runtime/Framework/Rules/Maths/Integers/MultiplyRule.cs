using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Maths.Integers
{

   
    [RuleMenu(Path = "Maths/Integer/Multiply")]
    public class MultiplyRule : Rule<int>
    {
        public Port<int> Input1 { get; set; }
        public Port<int> Input2 { get; set; }
        public sealed override Func<int> Logic { get; set; }

        public MultiplyRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value1);
                Circuit.GetValue(Input2, out var value2);

                var value = value1 * value2;
                SetLastValue(value);
                return lastValue;
            };
            Input1 = Port<int>.Create("Input 1", this);
            Input2 = Port<int>.Create("Input 2", this);
            AcceptedTypes.Add(typeof(Int16));
            AcceptedTypes.Add(typeof(Int32));
            AcceptedTypes.Add(typeof(Int64));
            AcceptedTypes.Add(typeof(Single));
            AcceptedTypes.Add(typeof(Double));
        }
    }

}