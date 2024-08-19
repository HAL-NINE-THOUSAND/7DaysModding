using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{
    [RuleMenu(Path = "Gates/NOT")]
    public class NotRule : Rule<bool>
    {
        public NotRule()
        {
            RuleName = "Hal.Not";
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue, out var fromCache);
                if (fromCache) return lastValue;
                SetLastValue(!lastValue);
                return lastValue;
            };

            Input1 = Port<bool>.Create("NOT input 1", this);
        }

        public Port<bool> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
    }
}