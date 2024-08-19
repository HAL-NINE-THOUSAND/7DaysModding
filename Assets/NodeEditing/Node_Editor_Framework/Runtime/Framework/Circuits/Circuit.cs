using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public class Circuit
    {
        public static Action<Circuit> SaveCircuit;

        private bool dirty;

        private bool isInitialised;

        [JsonIgnore] public Action<int, int> OnRun;

        public Circuit()
        {
        }

        public Circuit(Action<int, int> onRunEvent)
        {
            OnRun = onRunEvent;
        }

        public Guid CircuitId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

        public string OwnerId { get; set; } = "";

        [JsonIgnore] public Dictionary<Guid, IRule> Rules { get; set; } = new();

        public Dictionary<Guid, Guid> Connections { get; set; } = new();

        public bool HasAnyOutputSet()
        {
            foreach (var rule in Rules.Values)
                if (rule.RuleType == RuleType.Output)
                {
                    var value = rule.GetValueAsObject();

                    if (value is bool && (bool)value) return true;
                }

            return false;
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

            foreach (var input in rule.Inputs) Connections.Remove(input.InputId);

            var toRemove = new List<Guid>();
            foreach (var key in Connections.Keys)
                if (Connections[key] == rule.RuleId)
                    toRemove.Add(key);

            foreach (var id in toRemove)
                Connections.Remove(id);
        }

        public bool IsInputConnected(Guid inputId)
        {
            return Connections.ContainsKey(inputId);
        }

        public bool IsInputRule(Guid inputId, IRule rule)
        {
            if (Connections.TryGetValue(inputId, out var connRule)) return connRule == rule.RuleId;
            return false;
        }

        public bool RegisterConnection(ref IPort input, IRule rule, out string msg)
        {
            msg = string.Empty;
            if (input == null || rule == null)
                return false;

            if (IsSelfReference(rule, input.InputId))
            {
                msg = "Connection would cause signal loop";
                return false;
            }

            var inputId = input.InputId;
            if (!rule.IsCompatible(input.PortType))
            {
                if (input.Rule.TryConvert(rule.OutputType, out var newRule, out var convertMessage))
                {
                    newRule.CopyDetailsFromRule(input.Rule);
                    Rules[input.Rule.RuleId] = newRule;
                    input = newRule.Inputs.First(d => d.InputId == inputId);
                    newRule.SetCircuit(this);
                    msg = "Converted to " + newRule.RuleName;
                }
                else
                {
                    if (convertMessage != "")
                    {
                        msg = convertMessage;
                        return false;
                    }

                    msg = $"Input and rule types don't match. Output type must be {input.PortType.Name} but you tried to use {rule.OutputType.Name}";
                    return false;
                }
            }


            if (!input.Rule.Accepts(rule.OutputType))
            {
                msg = "This node won't accept that input";
                return false;
            }

            if (Connections.ContainsKey(input.InputId)) RemoveConnection(input);

            if (rule.Inputs.Any(d => d.InputId == inputId))
            {
                msg = "Input is already connected to this rule";
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
            if (Connections.TryGetValue(input.InputId, out var ruleId)) return Rules[ruleId];
            return null;
        }

        public void ResetValue<T>(Guid ruleId)
        {
            if (!RuleOutputCache<T>.Circuits.ContainsKey(CircuitId))
                RuleOutputCache<T>.Circuits.Add(CircuitId, new Dictionary<Guid, T>());

            var cache = RuleOutputCache<T>.Circuits[CircuitId];
            cache.Remove(ruleId);
        }

        public bool GetValue<T>(Guid inputId, out T value, out bool fromCache)
        {
            if (Connections.TryGetValue(inputId, out var ruleId))
                if (Rules.TryGetValue(ruleId, out var rule))
                {
                    if (RuleOutputCache<T>.Circuits.TryGetValue(rule.Circuit.CircuitId, out var circuitCache))
                        if (circuitCache.TryGetValue(rule.RuleId, out var ret))
                        {
                            value = ret;
                            fromCache = true;
                            return true;
                        }

                    //var t = typeof(T);
                    var castRule = rule as IRule<T>;
                    if (castRule == null)
                        value = (T)rule.GetValueAsObject();
                    else
                        value = castRule.GetValue();

                    fromCache = false;
                    return true;
                }

            //throw new NotImplementedException("Input ID not found. The world's gone mad - mad I say!");
            value = default;
            fromCache = false;
            return true;
        }

        public bool GetValue<T>(Port<T> input, out T value, out bool fromCache)
        {
            return GetValue(input.InputId, out value, out fromCache);
        }

        public bool GetValue<T>(Port<T> input, out T value)
        {
            return GetValue(input.InputId, out value, out _);
        }

        public void MarkCircuitAsDirty()
        {
            dirty = true;
        }

        public bool IsSelfReference(IRule rule, Guid inputId)
        {
            var queue = new Queue<IRule>();

            foreach (var node in rule.Inputs) queue.Enqueue(node.Rule);

            while (queue.TryDequeue(out var node))
                foreach (var child in node.Inputs)
                {
                    if (child.InputId == inputId)
                        return true;

                    if (Connections.TryGetValue(child.InputId, out var ruleId))
                        if (Rules.TryGetValue(ruleId, out var childRule))
                            queue.Enqueue(childRule);
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

            var outCount = 0;
            foreach (var node in Rules.Values)
                if (node.RuleType == RuleType.Output)
                {
                    node.GetValueAsObject();
                    outCount++;
                }

            //if no outputs, find any rules without outputs
            if (outCount == 0)
                foreach (var node in Rules.Values)
                    if (!Connections.ContainsValue(node.RuleId))
                    {
                        node.GetValueAsObject();
                        outCount++;
                    }

            var took = (int)(DateTime.Now - start).TotalMilliseconds;
            if (OnRun != null)
                OnRun(took, outCount);
            return took;
        }

        public void ResetCircuit()
        {
            foreach (var rule in Rules.Values)
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
            for (var x = 0; x < ruleCount; x++)
            {
                var ruleId = reader.ReadString();
                var rule = RulesManager.CreateRule(ruleId);
                rule.Read(reader);
                AddRule(rule);
            }

            var connectionCount = reader.ReadInt32();
            for (var x = 0; x < connectionCount; x++) Connections.Add(Guid.Parse(reader.ReadString()), Guid.Parse(reader.ReadString()));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CircuitId.ToString());
            writer.Write(Name);
            writer.Write(OwnerId);
            writer.Write(Rules.Count);
            foreach (var rule in Rules.Values) rule.Write(writer);

            writer.Write(Connections.Count);
            foreach (var connection in Connections)
            {
                writer.Write(connection.Key.ToString());
                writer.Write(connection.Value.ToString());
            }
        }
    }
}

public enum CircuitValueResult : byte
{
    Error = 0,
    Generated = 1,
    FromCache = 2,
    NotFound = 3
}