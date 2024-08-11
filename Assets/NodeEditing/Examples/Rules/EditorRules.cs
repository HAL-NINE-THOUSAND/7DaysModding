using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

#if UNITY_EDITOR
[RuleMenu(Path = "Input/Integer")]
[RuleTitle(Title = "Int")]
public class IntegerInputRule : Rule<int>
{
    public IntegerInputRule()
    {
        RuleType = RuleType.Processor;
        Logic = () => { return lastValue = Target; };
    }

    public sealed override Func<int> Logic { get; set; }
    public int Target { get; set; }

    public override void DrawUI()
    {
        Target = RTEditorGUI.IntField(new GUIContent("Value", "The input value of type integer"), Target, MarkCircuitAsDirty);
    }
}

#endif