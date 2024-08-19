using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeEditorFramework;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public enum InputPosition : byte
    {
        Left = 0,
        Top = 1
    }

    public static class RuleOutputCache<T>
    {
        public static Dictionary<Guid, Dictionary<Guid, T>> Circuits { get; set; } = new();
    }

    public static class RuleInputCache<T>
    {
        public static Dictionary<Guid, Dictionary<string, T>> Circuits { get; set; } = new();


        public static void Add(Guid circuitId, string inputKey, T value)
        {
            if (!Circuits.TryGetValue(circuitId, out var dic))
            {
                dic = new Dictionary<string, T>();
                Circuits.Add(circuitId, dic);
            }

            dic.Remove(inputKey);
            dic.Add(inputKey, value);
        }

        public static bool TryGet(Guid circuitId, string inputKey, out T value)
        {
            if (!Circuits.TryGetValue(circuitId, out var circuit))
            {
                value = default;
                return false;
            }

            return circuit.TryGetValue(inputKey, out value);
        }
    }

    public enum RuleType
    {
        Unset = 0,
        Input = 1,
        Processor = 2,
        Output = 3
    }


    public interface IRule
    {
        string RuleName { get; set; }
        Guid RuleId { get; set; }

        NodeSide InputPosition { get; set; }

        NodeSide OutputPosition { get; set; }

        //string InputKey { get; set; }
        Vector2 Position { get; set; }

        Circuit Circuit { get; set; }
        public List<IPort> Inputs { get; set; }

        RuleType RuleType { get; }
        public string TypeName { get; }

        public Type OutputType { get; set; }
        public void SetCircuit(Circuit parent);

        public object GetValueAsObject();

        public IRule CreateNew();
        public void ResetValue();

        public Type GetOutputType();

        public object GetLastValue();

        public bool IsCompatible(Type other);
        public bool Accepts(Type other);

        public void DrawUI();
        public bool TryConvert(Type type, out IRule rule, out string message);
        public void MarkCircuitAsDirty();

        public void Write(BinaryWriter writer);
        public void Read(BinaryReader reader);
        public void CopyDetailsFromRule(IRule rule);
    }

    public interface IRule<out T> : IRule
    {
        public T GetValue();

        public new T GetLastValue();
    }

    public abstract class Rule<T> : IRule<T>
    {
        protected T lastValue;

        public abstract Func<T> Logic { get; set; }


        //need to move this to a separate object/helper class so we're not duplicating the data everywhere
        public Dictionary<Type, string> ConvertTypes { get; set; } = new(0);

        public NodeSide InputPosition { get; set; } = NodeSide.Left;

        public NodeSide OutputPosition { get; set; } = NodeSide.Right;
        public string RuleName { get; set; }

        public Guid RuleId { get; set; } = Guid.NewGuid();

        //public string InputKey { get; set; } = string.Empty;

        public Vector2 Position { get; set; }

        public Type OutputType { get; set; } = typeof(T);
        public Circuit Circuit { get; set; }

        public List<IPort> Inputs { get; set; } = new();

        public RuleType RuleType { get; protected set; }

        public virtual void CopyDetailsFromRule(IRule rule)
        {
            for (var index = 0; index < rule.Inputs.Count; index++)
            {
                var copyFrom = rule.Inputs[index];
                var copyTo = Inputs[index];
                copyTo.InputId = copyFrom.InputId;
            }

            var sourceProps = rule.GetType().GetProperties();
            var thisProps = GetType().GetProperties();

            foreach (var prop in sourceProps)
            {
                var thisProp = thisProps.FirstOrDefault(d => d.Name == prop.Name);
                if (thisProp == null)
                    continue;

                if (!thisProp.PropertyType.IsPrimitive)
                    continue;

                var val = prop.GetValue(rule);
                try
                {
                    var newValue = Convert.ChangeType(val, thisProp.PropertyType);
                    thisProp.SetValue(this, newValue);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Set prop failed on {thisProp.Name}: " + val);
                }
            }
            Position = rule.Position;
            RuleId = rule.RuleId;
            InputPosition = rule.InputPosition;
            OutputPosition = rule.OutputPosition;
        }

        public bool TryConvert(Type type, out IRule rule, out string message)
        {
            foreach (var input in Inputs)
            {
                if (Circuit.Connections.TryGetValue(input.InputId, out var connectedRule))
                {
                    if (Circuit.Rules[connectedRule].Inputs.Count > 1)
                    {
                        message = "Can not convert when inputs are connected, disconnect all inputs and try again";
                        rule = null;
                        return false;
                    }
                }
                
                if (Inputs.Count > 1)
                {
                    if (Circuit.Connections.ContainsKey(input.InputId))
                    {
                        message = "Can not convert when inputs are connected, disconnect all inputs and try again";
                        rule = null;
                        return false;
                    }
                }
                
            }

            rule = null;
            message = string.Empty;
            if (ConvertTypes.TryGetValue(type, out var ruleName))
            {
                rule = RulesManager.CreateRule(ruleName);
                return rule != null;
            }

            return false;
        }

        public T GetValue()
        {
            var ret = Logic();
            return ret;
        }

        public T GetLastValue()
        {
            return lastValue;
        }


        public virtual void DrawUI()
        {
        }


        public void MarkCircuitAsDirty()
        {
            Circuit.MarkCircuitAsDirty();
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(RuleName);
            writer.Write(RuleId.ToString());
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write((byte)InputPosition);
            writer.Write((byte)OutputPosition);
            writer.Write((int)RuleType);
            writer.Write(Inputs.Count);
            foreach (var input in Inputs) writer.Write(input.InputId.ToString());
        }

        public virtual void Read(BinaryReader reader)
        {
            //RuleName will be reader to instantiate this class to Read()
            //var version = reader.ReadInt16();
            RuleId = Guid.Parse(reader.ReadString());
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            InputPosition = (NodeSide)reader.ReadByte();
            OutputPosition = (NodeSide)reader.ReadByte();
            RuleType = (RuleType)reader.ReadInt32();
            var inputCount = reader.ReadInt32();
            for (var x = 0; x < inputCount; x++)
            {
                var inputId = Guid.Parse(reader.ReadString());
                Inputs[x].InputId = inputId;
            }
        }

        public bool IsCompatible(Type other)
        {
            if (OutputType == other)
                return true;

            if (OutputType.IsAssignableFrom(other))
                return true;

            if (other.IsAssignableFrom(OutputType))
                return true;

            return false;
        }

        public bool Accepts(Type other)
        {
            return true;
        }

        public void ResetValue()
        {
            Circuit.ResetValue<T>(RuleId);
        }

        public virtual Type GetOutputType()
        {
            return typeof(T);
        }

        object IRule.GetLastValue()
        {
            return GetLastValue();
        }

        public void SetCircuit(Circuit parent)
        {
            Circuit = parent;
        }

        public string TypeName => GetType().Name;

        public object GetValueAsObject()
        {
            return GetValue();
        }

        public IRule CreateNew()
        {
            return Activator.CreateInstance(GetType()) as IRule;
        }


        public void SetLastValue(T value)
        {
            lastValue = value;
            RuleOutputCache<T>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
        }
    }
}