using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{
    [RuleMenu(Path = "Gates/AND")]
    public class AndRule : Rule<bool>
    {
        public AndRule()
        {
            RuleName = "Hal.And";
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue, out _);
                if (!lastValue)
                {
                    SetLastValue(false);
                    return false;
                }

                Circuit.GetValue(Input2, out lastValue, out _);

                SetLastValue(lastValue);
                return lastValue;
            };

            Input1 = Port<bool>.Create("AND input 1", this);
            Input2 = Port<bool>.Create("AND input 2", this);
        }

        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
    }
}