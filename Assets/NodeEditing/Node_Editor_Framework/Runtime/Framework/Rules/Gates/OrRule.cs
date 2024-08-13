using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{

    [RuleMenu(Path = "Gates/OR")]
    public class OrRule : Rule<bool>
    {
        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        public OrRule()
        {
            RuleId = "Hal.Or";
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue);
                if (lastValue)
                {
                    SetLastValue(lastValue);
                    return true;
                }
                Circuit.GetValue(Input2, out lastValue);
                SetLastValue(lastValue);
                return lastValue;
            };
            Input1 = Port<bool>.Create("OR input 1", this);
            Input2 = Port<bool>.Create("OR input 2", this);
        }
    }

}