using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{
    [RuleMenu(Path = "Gates/OR")]
    public class OrRule : Rule<bool>
    {
        public OrRule()
        {
            RuleName = "Hal.Or";
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue, out _);
                if (lastValue)
                {
                    SetLastValue(lastValue);
                    return true;
                }

                Circuit.GetValue(Input2, out lastValue, out _);
                SetLastValue(lastValue);
                return lastValue;
            };
            Input1 = Port<bool>.Create("OR input 1", this);
            Input2 = Port<bool>.Create("OR input 2", this);
        }

        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
    }
}