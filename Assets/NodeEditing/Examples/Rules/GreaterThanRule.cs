
    using System;
    using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
    using NodeEditorFramework.Utilities;
    using UnityEngine;

    [RuleMenu(Path = "Logic/Greater Than")]
    [RuleTitle(Title="Greater Than")]
    public class GreaterThanRule : Rule<bool>
    {
        public Port<int> Input1 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        public int Target { get; set; }
        
        public GreaterThanRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out var value);
                lastValue = value > Target;
                RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                return lastValue;
            };
            Input1 = Port<int>.Create("greater than", this);
            
        }
        
        public override void DrawUI()
        {
            Target = RTEditorGUI.IntField(new GUIContent("Value", "Target Value"), Target, MarkCircuitAsDirty);
        }
    }
    
    
    [RuleMenu(Path = "Logic/OR")]
    public class OrRule : Rule<bool>
    {
        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        public OrRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue);
                if (lastValue)
                {
                    RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                    return true;
                }
                Circuit.GetValue(Input2, out lastValue);
                RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                return lastValue;
            };
            Input1 = Port<bool>.Create("OR input 1", this);
            Input2 = Port<bool>.Create("OR input 2", this);
        }
    }
    
    
    [RuleMenu(Path = "Logic/XOR")]
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
                RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                return lastValue;
            };
            Input1 = Port<bool>.Create("OR input 1", this);
            Input2 = Port<bool>.Create("OR input 2", this);
        }
    }
    
    
    [RuleMenu(Path = "Logic/AND")]
    public class AndRule : Rule<bool>
    {
        public Port<bool> Input1 { get; set; }
        public Port<bool> Input2 { get; set; }
        public sealed override Func<bool> Logic { get; set; }
        
        public AndRule()
        {
            RuleType = RuleType.Processor;
            Logic = () =>
            {
                Circuit.GetValue(Input1, out lastValue);
                if (!lastValue)
                {
                    RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, false);
                    return false;
                }
                Circuit.GetValue(Input2, out lastValue);
                
                RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                return lastValue;
            };
            
            Input1 = Port<bool>.Create("AND input 1", this);
            Input2 = Port<bool>.Create("AND input 2", this);
        }
    }
    
    
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
                RuleOutputCache<bool>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
                return lastValue;
            };
            
            Input1 = Port<object>.Create("Input 1", this);
        }
        
        
    }