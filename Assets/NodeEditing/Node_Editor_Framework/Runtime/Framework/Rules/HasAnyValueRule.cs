#if UNITY_EDITOR
using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

[RuleMenu(Path = "Results/Any")]
public class HasAnyValueRule : Rule<bool>
{
    public HasAnyValueRule()
    {
        RuleName = "Hal.Any";
        RuleType = RuleType.Output;
        Logic = () =>
        {
            Circuit.GetValue(Input1, out var value, out _);

            lastValue = value is bool ? (bool)value : value != null;
            SetLastValue(lastValue);
            return lastValue;
        };

        Input1 = Port<object>.Create("Input 1", this);
    }

    public Port<object> Input1 { get; set; }
    public sealed override Func<bool> Logic { get; set; }
}

#endif