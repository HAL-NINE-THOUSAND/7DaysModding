using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public static class RuleOutputCache<T>
    {
        public static Dictionary<Guid, Dictionary<string, T>> Circuits { get; set; } = new();
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
        string RuleId { get; set; }

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
        
        public List<Type> AcceptedTypes { get; set; }

        public void MarkCircuitAsDirty();

        public abstract void Write(BinaryWriter writer);
        public abstract void Read(BinaryReader reader);
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
        public string RuleId { get; set; } 

        public Vector2 Position { get; set; }

        public Type OutputType { get; set; } = typeof(T);
        public Circuit Circuit { get; set; }

        public List<IPort> Inputs { get; set; } = new();

        public RuleType RuleType { get; protected set; }

        public T GetValue()
        {
            var ret = Logic();
            return ret;
        }


        public void SetLastValue(T value)
        {
            lastValue = value;
            RuleOutputCache<T>.Circuits[Circuit.CircuitId].TryAdd(RuleId, lastValue);
        }

        public T GetLastValue()
        {
            return lastValue;
        }


        public virtual void DrawUI()
        {
            
        }

        public List<Type> AcceptedTypes { get; set; } = new(0);

        public void MarkCircuitAsDirty()
        {
            Circuit.MarkCircuitAsDirty();
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(RuleId);
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write((int)RuleType);
            writer.Write(Inputs.Count);
            foreach (var input in Inputs)
            {
                writer.Write(input.InputId.ToString());
            }
        }

        public virtual void Read(BinaryReader reader)
        {
            //RuleId will be reader to instantiate this class to Read()
            //RuleId = Guid.Parse(reader.ReadString());
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            RuleType = (RuleType)reader.ReadInt32();
            
            var inputCount = reader.ReadInt32();
            for (int x = 0; x < inputCount; x++)
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
            if (AcceptedTypes.Count > 0 && !AcceptedTypes.Contains(other))
                return false;
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
    }
}