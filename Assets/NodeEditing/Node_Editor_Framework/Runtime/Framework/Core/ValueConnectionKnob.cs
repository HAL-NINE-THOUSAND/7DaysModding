﻿using System;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework
{
    [Serializable]
    public class ValueConnectionKnob : ConnectionKnob
    {
        [NonSerialized] private object _value;
        // Connections
        //new public List<ValueConnectionKnob> connections { get { return _connections.OfType<ValueConnectionKnob> ().ToList (); } }

        // Knob Style
        protected override Type styleBaseClass => typeof(ValueConnectionType);

        protected new ValueConnectionType ConnectionStyle
        {
            get
            {
                CheckConnectionStyle();
                return (ValueConnectionType)_connectionStyle;
            }
        }

        // Knob Value
        public Type valueType => ConnectionStyle.Type;
        public bool IsValueNull => value == null;

        private object value
        {
            get => _value;
            set
            {
                _value = value;
                if (direction == Direction.Out)
                {
                    // foreach (ValueConnectionKnob connectionKnob in connections)
                    // 	connectionKnob.SetValue(value);
                }
            }
        }

        public void Init(IPort port, Node node, string name, Direction dir, string type)
        {
            base.Init(port, node, name, dir);
            styleID = type;
        }

        public void Init(IPort port, Node node, string name, Direction dir, string type, NodeSide nodeSide, float nodeSidePosition = 0)
        {
            base.Init(port, node, name, dir, nodeSide, nodeSidePosition);
            styleID = type;
        }

        // new public ValueConnectionKnob connection (int index) 
        // {
        // 	if (connections.Count <= index)
        // 		throw new IndexOutOfRangeException ("connections[" + index + "] of '" + name + "'");
        // 	return connections[index];
        // }

        public override bool CanApplyConnection(ConnectionPort port)
        {
            var valueKnob = port as ValueConnectionKnob;
            if (valueKnob == null || !valueType.IsAssignableFrom(valueKnob.valueType))
                return false;
            return base.CanApplyConnection(port);
        }

        #region Knob Value

        /// <summary>
        ///     Gets the knob value anonymously. Not advised as it may lead to unwanted behaviour!
        /// </summary>
        public object GetValue()
        {
            return value;
        }

        /// <summary>
        ///     Gets the output value if the type matches or null. If possible, use strongly typed version instead.
        /// </summary>
        public object GetValue(Type type)
        {
            if (type == null)
                throw new ArgumentException("Trying to GetValue of knob " + name + " with null type!");
            if (type.IsAssignableFrom(valueType))
                return value ?? (value = GetDefault(type));
            throw new ArgumentException("Trying to GetValue of type " + type.FullName + " for Output Type: " + valueType.FullName);
        }

        /// <summary>
        ///     Sets the output value if the type matches. If possible, use strongly typed version instead.
        /// </summary>
        public void SetValue(object Value)
        {
            if (Value != null && !valueType.IsAssignableFrom(Value.GetType()))
                throw new ArgumentException("Trying to SetValue of type " + Value.GetType().FullName + " for Output Type: " + valueType.FullName);
            value = Value;
        }

        /// <summary>
        ///     Gets the output value if the type matches
        /// </summary>
        /// <returns>Value, if null default(T) (-> For reference types, null. For value types, default value)</returns>
        public T GetValue<T>()
        {
            if (typeof(T).IsAssignableFrom(valueType))
                return (T)(value ?? (value = GetDefault<T>()));
            Debug.LogError("Trying to GetValue<" + typeof(T).FullName + "> for Output Type: " + valueType.FullName);
            return GetDefault<T>();
        }

        /// <summary>
        ///     Sets the output value if the type matches
        /// </summary>
        public void SetValue<T>(T Value)
        {
            if (valueType.IsAssignableFrom(typeof(T)))
                value = Value;
            else
                Debug.LogError("Trying to SetValue<" + typeof(T).FullName + "> for Output Type: " + valueType.FullName);
        }

        /// <summary>
        ///     Resets the output value to null.
        /// </summary>
        public void ResetValue()
        {
            value = null;
        }

        /// <summary>
        ///     Returns the default value of type when a default constructor is existant or type is a value type, else null
        /// </summary>
        private static T GetDefault<T>()
        {
            // Try to create using an empty constructor if existant
            if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance<T>();
            // Else try to get default. Returns null only on reference types
            return default;
        }

        /// <summary>
        ///     Returns the default value of type when a default constructor is existant, else null
        /// </summary>
        private static object GetDefault(Type type)
        {
            // Try to create using an empty constructor if existant
            if (type.GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance(type);
            return null;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ValueConnectionKnobAttribute : ConnectionKnobAttribute
    {
        public Type ValueType;

        public ValueConnectionKnobAttribute(string name, Direction direction, string type)
            : base(name, direction, type)
        {
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, string type, ConnectionCount maxCount)
            : base(name, direction, type, maxCount)
        {
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, string type, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, direction, type, nodeSide, nodeSidePos)
        {
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, string type, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, direction, type, maxCount, nodeSide, nodeSidePos)
        {
        }

        // Directly typed
        public ValueConnectionKnobAttribute(string name, Direction direction, Type type)
            : base(name, direction)
        {
            Setup(type);
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, Type type, ConnectionCount maxCount)
            : base(name, direction, maxCount)
        {
            Setup(type);
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, Type type, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, direction, nodeSide, nodeSidePos)
        {
            Setup(type);
        }

        public ValueConnectionKnobAttribute(string name, Direction direction, Type type, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, direction, maxCount, nodeSide, nodeSidePos)
        {
            Setup(type);
        }

        public override Type ConnectionType => typeof(ValueConnectionKnob);

        protected void Setup(Type type)
        {
            StyleID = type.FullName;
            ValueType = type;
            ConnectionPortStyles.GetValueConnectionType(type);
        }

        public override bool IsCompatibleWith(ConnectionPort port)
        {
            if (!(Direction == Direction.None && port.direction == Direction.None)
                && !(Direction == Direction.In && port.direction == Direction.Out)
                && !(Direction == Direction.Out && port.direction == Direction.In))
                return false;
            var valueKnob = port as ValueConnectionKnob;
            if (valueKnob == null)
                return false;
            var knobType = ConnectionPortStyles.GetValueType(StyleID);
            return knobType.IsAssignableFrom(valueKnob.valueType);
        }

        public override ConnectionPort CreateNew(IPort port, Node node)
        {
            var knob = ScriptableObject.CreateInstance<ValueConnectionKnob>();
            knob.Init(port, node, Name, Direction, StyleID, NodeSide, NodeSidePos);
            knob.maxConnectionCount = MaxConnectionCount;
            return knob;
        }

        public override void UpdateProperties(ConnectionPort port)
        {
            var knob = (ValueConnectionKnob)port;
            knob.name = Name;
            knob.direction = Direction;
            knob.styleID = StyleID;
            knob.maxConnectionCount = MaxConnectionCount;
            knob.inputSide = NodeSide;
            if (NodeSidePos != 0)
                knob.sidePosition = NodeSidePos;
            knob.sideOffset = 0;
        }
    }

    [ReflectionUtility.ReflectionSearchIgnore]
    public class ValueConnectionType : ConnectionKnobStyle
    {
        protected Type type;

        public ValueConnectionType()
        {
        }

        public ValueConnectionType(Type valueType) : base(valueType.FullName)
        {
            identifier = valueType.FullName;
            type = valueType;
        }

        public virtual Type Type => type;

        public override bool isValid()
        {
            var valid = Type != null && InKnobTex != null && OutKnobTex != null;
            if (!valid)
                Debug.LogError("Type " + Identifier + " is invalid! Type-Null?" + (type == null) + ", InTex-Null?" + (InKnobTex == null) + ", OutTex-Null?" + (OutKnobTex == null));
            return valid;
        }
    }
}