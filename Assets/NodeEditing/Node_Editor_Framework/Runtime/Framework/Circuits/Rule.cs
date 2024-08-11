using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public static class RuleOutputCache<T>
    {
        public static Dictionary<Guid, Dictionary<Guid, T>> Circuits { get; set; } = new();
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
        Guid RuleId { get; set; }

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

        public void DrawUI();

        public void MarkCircuitAsDirty();
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
        public Guid RuleId { get; set; } = Guid.NewGuid();

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