#if UNITY_EDITOR
    using System;
    using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
    using NodeEditorFramework.Utilities;
    using UnityEngine;
    
    [RuleMenu(Path = "Results/Any")]
    public class HasAnyValueRule : Rule<bool>
    {
        public Port<object> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        
        public HasAnyValueRule()
        {
            RuleType = RuleType.Output;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out object value);

                lastValue = value is bool ? (bool)value : value != null;
                SetLastValue(lastValue);
                return lastValue;
            };
            
            Input1 = Port<object>.Create("Input 1", this);
        }
        
        
    }
    
#endif