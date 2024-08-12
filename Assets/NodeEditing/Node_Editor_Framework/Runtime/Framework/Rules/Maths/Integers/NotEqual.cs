using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Rules.Maths.Integers
{

   
    [RuleMenu(Path = "Maths/Integer/Not Equal")]
    [RuleTitle(Title="!=")]
    public class NotEqualRule : Rule<bool>
    {
        public Port<int> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        public int Target { get; set; }
        
        public NotEqualRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value);
                SetLastValue(value != Target);
                return lastValue;
            };
            Input1 = Port<int>.Create("!=", this);
            
        }
        
        public override void DrawUI()
        {
            Target = RTEditorGUI.IntField(new GUIContent("Value", "Target Value"), Target, MarkCircuitAsDirty);
        }
    }


}