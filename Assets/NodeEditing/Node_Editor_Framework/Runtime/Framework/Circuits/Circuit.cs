using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public class Circuit
    {

        public Guid CircuitId = Guid.NewGuid();

        public Dictionary<Guid, IRule> Rules { get; set; } = new();


        public Dictionary<Guid, Guid> Connections { get; set; } = new();

        private bool isInitialised;

        public Action<int> OnRun;

        public Circuit()
        {
            
        }

        public Circuit(Action<int> onRunEvent)
        {
            OnRun = onRunEvent;
        }
        
        public void AddRule(IRule rule)
        {
            Rules.Add(rule.RuleId, rule);
            rule.SetCircuit(this);
        }

        public bool IsInputConnected(Guid inputId)
        {
            return Connections.ContainsKey(inputId);
        }
        public bool RegisterConnection(IPort input, IRule rule, out string msg)
        {
            msg = string.Empty;
            if (input == null || rule == null)
                return false;

            if (IsSelfReference(rule, input.InputId))
            {
                msg = "Connection would cause signal loop";
                return false;
            }
            
            if (!rule.IsCompatible(input.PortType))
            {
                msg = $"Input and rule types don't match. Output type must be {input.PortType.Name}";
                return false;
            }

            if (Connections.ContainsKey(input.InputId))
            {
                msg = $"Input already has a connection";
                return false;
            }

            if (rule.Inputs.Any(d => d.InputId == input.InputId))
            {
                msg = $"Input is already connected to this rule";
                return false;
            }
            
            Connections.Remove(input.InputId);
            Connections.Add(input.InputId, rule.RuleId);
            return true;
        }

        public void RemoveConnection(Guid inputId)
        {
            Connections.Remove(inputId);
        }
        public void RemoveConnection(IPort input)
        {
            Connections.Remove(input.InputId);
        }

        public IRule GetConnectionRule(IPort input)
        {
            if (Connections.TryGetValue(input.InputId, out var ruleId))
            {
                return Rules[ruleId];
            }
            return null;
        }

        public void ResetValue<T>(Guid inputId)
        {
            if (!RuleOutputCache<T>.Circuits.ContainsKey(CircuitId))
                RuleOutputCache<T>.Circuits.Add(CircuitId, new());
            
            var cache = RuleOutputCache<T>.Circuits[CircuitId];
            cache.Remove(inputId);
        }
        
        public bool GetValue<T>(Guid inputId, out T value)
        {
            if (Connections.TryGetValue(inputId, out var ruleId))
            {
                if (Rules.TryGetValue(ruleId, out var rule))
                {
                    if (RuleOutputCache<T>.Circuits.TryGetValue(rule.Circuit.CircuitId, out var circuitCache))
                    {
                        if (circuitCache.TryGetValue(rule.RuleId, out var ret))
                        {
                            value = ret;
                            return true;
                        }
                    }

                    var castRule = rule as IRule<T>;
                    if (castRule == null)
                        value = (T)rule.GetValueAsObject();
                    else 
                        value = castRule.GetValue();
                    
                    return true;
                }
            }

            //throw new NotImplementedException("Input ID not found. The world's gone mad - mad I say!");
            value = default;
            return false;
        }

        public bool GetValue<T>(Port<T> input, out T value)
        {
            return GetValue(input.InputId, out value);
        }

        private bool dirty = false;
        public void MarkCircuitAsDirty()
        {
            dirty = true;
        }

        public bool IsSelfReference(IRule rule, Guid inputId)
        {

            var queue = new Queue<IRule>();

            foreach (var node in rule.Inputs)
            {
                queue.Enqueue(node.Node);
            }
            
            while (queue.TryDequeue(out var node))
            {
                foreach (var child in node.Inputs)
                {
                    if (child.InputId == inputId)
                        return true;
                    
                    if (Connections.TryGetValue(child.InputId, out var ruleId))
                        if (Rules.TryGetValue(ruleId, out var childRule))
                            queue.Enqueue(childRule);
                }
            }

            return false;
        }
        
        public void RunIfDirty()
        {
            if (!dirty)
                return;

            dirty = false;
            Run();
        }

        public int Run()
        {

            var start = DateTime.Now;
            ResetCircuit();
            foreach (var node in Rules.Values)
            {
                if (node.RuleType == RuleType.Output)
                {
                    node.GetValueAsObject();
                }
            }

            var took = (int)((DateTime.Now) - start).TotalMilliseconds;
            if (OnRun != null)
                OnRun(took);
            return took;
        }
        
        public void ResetCircuit()
        {
            
            foreach(var rule in Rules.Values)
                rule.ResetValue();
        }

        public void Init()
        {

            if (isInitialised)
                return;
            isInitialised = true;

            ResetCircuit();
        }

    }
}