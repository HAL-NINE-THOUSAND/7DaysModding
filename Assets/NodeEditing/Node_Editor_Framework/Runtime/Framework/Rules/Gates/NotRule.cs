using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework.Rules.Gates
{

    [RuleMenu(Path = "Gates/NOT")]
    public class NotRule : Rule<bool>
    {
        public Port<bool> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        
        public NotRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                var found = Circuit.GetValue(Input1, out lastValue);
                if (found)
                {
                    return lastValue;
                }
                SetLastValue(!lastValue);
                return lastValue;
            };
            
            Input1 = Port<bool>.Create("NOT input 1", this);
        }
    }


}