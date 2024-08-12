using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{

  
    [RuleMenu(Path = "Gates/XOR")]
    public class XorRule : Rule<bool>
    {
        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        public XorRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var a);
                Circuit.GetValue(Input2, out var b);
                lastValue =  a ^ b;
                SetLastValue(lastValue);
                return lastValue;
            };
            Input1 = Port<bool>.Create("OR input 1", this);
            Input2 = Port<bool>.Create("OR input 2", this);
        }
    }

}