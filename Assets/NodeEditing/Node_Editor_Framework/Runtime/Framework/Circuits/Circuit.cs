using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public class Circuit
    {

        public static Action<Circuit> SaveCircuit;
        
        public Guid CircuitId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

        public string OwnerId { get; set; } = "";

        [JsonIgnore]
        public Dictionary<string, IRule> Rules { get; set; } = new();
        public Dictionary<Guid, string> Connections { get; set; } = new();

        private bool isInitialised;

        [JsonIgnore]
        public Action<int> OnRun;

        public Circuit()
        {
            
        }
        
        public Circuit(Action<int> onRunEvent)
        {
            OnRun = onRunEvent;
        }

        public void Build()
        {
            // foreach (var ruleId in RulesIds)
            // {
            //     var rule = RulesManager.CreateRule(ruleId);
            //     rule.SetCircuit(this);
            //     Rules.Add(ruleId, rule);
            // }
        }
        
        public void AddRule(IRule rule)
        {
            if (!Rules.ContainsKey(rule.RuleId))
                Rules.Add(rule.RuleId, rule);
            rule.SetCircuit(this);
            //RulesIds.Add(rule.RuleId);
        }
        
        public void RemoveRule(IRule rule)
        {
            Rules.Remove(rule.RuleId);
            //RulesIds.Remove(rule.RuleId);
        }

        public bool IsInputConnected(Guid inputId)
        {
            return Connections.ContainsKey(inputId);
        }
        
        public bool IsInputRule(Guid inputId, IRule rule)
        {
            if (Connections.TryGetValue(inputId, out string connRule))
            {
                return connRule == rule.RuleId;
            }
            return false;
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
                msg = $"Input and rule types don't match. Output type must be {input.PortType.Name} but you tried to use {rule.OutputType.Name}";
                return false;
            }

            if (!input.Node.Accepts(rule.OutputType))
            {
                msg = $"This node won't accept that input";
                return false;
            }
            if (Connections.ContainsKey(input.InputId))
            {
                RemoveConnection(input);
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

        public void ResetValue<T>(string ruleId)
        {
            if (!RuleOutputCache<T>.Circuits.ContainsKey(CircuitId))
                RuleOutputCache<T>.Circuits.Add(CircuitId, new());
            
            var cache = RuleOutputCache<T>.Circuits[CircuitId];
            cache.Remove(ruleId);
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

        public void Read(BinaryReader reader)
        {
            CircuitId = Guid.Parse(reader.ReadString());
            Name = reader.ReadString();
            OwnerId = reader.ReadString();

            var ruleCount = reader.ReadInt32();
            for (int x = 0; x < ruleCount; x++)
            {
                var ruleId = reader.ReadString();
                var rule = RulesManager.CreateRule(ruleId);
                rule.Read(reader);
                AddRule(rule);
            }

            var connectionCount = reader.ReadInt32();
            for (int x = 0; x < connectionCount; x++)
            {
                Connections.Add(Guid.Parse(reader.ReadString()), reader.ReadString());
            }
        }
        public void Write(BinaryWriter writer)
        {

            writer.Write(CircuitId.ToString());
            writer.Write(Name);
            writer.Write(OwnerId);
            writer.Write(Rules.Count);
            foreach (var rule in Rules.Values)
            {
                rule.Write(writer);
            }

            writer.Write(Connections.Count);
            foreach (var connection in Connections)
            {
                writer.Write(connection.Key.ToString());
                writer.Write(connection.Value.ToString());
            }
        }
    }
}