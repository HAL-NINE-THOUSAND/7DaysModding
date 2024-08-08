using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NodeEditorFramework.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NodeEditorFramework
{
    // Define a common interface for ports with a generic method to get the value
    public interface IPort
    {
        Direction Direction { get; }
        Type PortType { get; }
        T GetValue<T>();
        void SetValue<T>(T value);
        void ResetValue();

        List<Connection> Connections { get; set; }
        bool IsSamePort(IPort port);
        Node Node { get; set; }
        
        bool Connected { get; }
        ConnectionKnob Knob { get; set; }
        public void AddConnection(Connection connection);
        public Connection RemoveConnection(Connection connection);
        public Connection RemoveConnection(IPort port);
        void DrawConnections();
        void DisplayLayout();
    }

// Define a generic Port class implementing IPort
    [DebuggerDisplay("Port: {name}: {Value}")]
    public class Port<T> : IPort
    {
        public T Value { get; set; }

        public T DisconnectedValue { get; set; }
        
        private string name;
        public string Name => name;

        public Node Node { get; set; }
        public ConnectionKnob Knob { get; set; }
        [NonSerialized]
        public Color color = Color.white;

        public Direction Direction { get; set; }

        public void ResetValue()
        {
            SetValue(DisconnectedValue);
        }

        public void SetValue<T1>(T1 value) 
        {
            if (!IsSamePort<T1>())
                return;
            
            Value = (T)(object)value; //boxing?
            
            if (Direction == Direction.Out)
            {
                foreach (var conn in Connections)
                {
                    if (conn.SourcePort == this)
                    {
                        conn.TargetPort.SetValue(Value);
                    }
                }
            }
            
        }

        public List<Connection> Connections { get; set; } = new();
        public Type PortType => typeof(T);
        
        public Port(string name, Direction direction, T disconnectedValue, Node parent)
        {
            this.name = name;
            this.Direction = direction;
            Value = DisconnectedValue = disconnectedValue;
            Node = parent;
            Knob = ScriptableObject.CreateInstance<ConnectionKnob>();
            Knob.Init(this, name, direction);
        }
        
        /// <summary>
        /// Draws a label with the given GUIContent and the given style. Places the knob next to it at it's nodeSide
        /// </summary>
        public void DisplayLayout ()
        {
            foreach(var conn in Connections)
            {
                
                Vector2 startPos = conn.SourcePort.Knob.GetGUIKnob ().center;
                Vector2 startDir = conn.SourcePort.Knob.GetDirection();
                Vector2 endPos = conn.TargetPort.Knob.GetGUIKnob().center;
                Vector2 endDir = conn.TargetPort.Knob.GetDirection();
                NodeEditorGUI.DrawConnection(endPos, endDir, startPos, startDir, color);
            }
        }
        
        public static Port<T> Create(string name, Direction direction, Node parent, T value)
        {
            var ret = new Port<T>(name, direction, value, parent);
            return ret;
        }

        public virtual void OnDisconnected()
        {
            //if (Direction == Direction.Out)
            SetValue(DisconnectedValue);
        }
        
        public bool Connected => Connections.Count > 0;
        
        public bool IsSamePort(IPort port)
        {
            
            if (PortType == port.PortType)
                return true;

            if (port.PortType.IsAssignableFrom(PortType))
                return true;
            
            if (PortType.IsAssignableFrom(port.PortType))
                return true;

            return false;
            //return port?.PortType == typeof(T);
        }
        public bool IsSamePort<TPortType>()
        {
            
            var other = typeof(TPortType);
            if (PortType == other)
                return true;

            if (other.IsAssignableFrom(PortType))
                return true;
            
            if (PortType.IsAssignableFrom(other))
                return true;

            return false;
        }
        
        public TValue GetValue<TValue>()
        {
            if (!IsSamePort<TValue>())
            {
                throw new InvalidOperationException($"Port is not of type {typeof(TValue)}.");
            }

            if (Direction == Direction.In && !Connected)
                return (TValue)(object)DisconnectedValue;
            
            return (TValue)(object)Value;
        }



        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);
        }
        public Connection RemoveConnection(IPort port)
        {
            var conn = Connections.FirstOrDefault(d => d.TargetPort == port || d.SourcePort == port);
            if (conn == null)
            {
                Debug.LogError("Tried to disconnect a connection that doesn't exist. Do better next time eh?");
                return null;
            }
            return RemoveConnection(conn);
        }

        public Connection RemoveConnection(Connection conn)
        {

            var list = Direction == Direction.In ? Node.IncomingConnections : Node.OutgoingConnections;

            OnDisconnected();
            
            Connections.Remove(conn);
            for (int x = 0; x < list.Count; x++)
            {
                if (list[x] == conn)
                {
                    list.RemoveAt(x);

                    if (conn.SourcePort == this)
                        conn.TargetPort.RemoveConnection(conn);
                    else
                        conn.SourcePort.RemoveConnection(conn);
                    return conn;
                }
            }

            return null;
        }
        /// <summary>
        /// Draws the connection curves from this knob to all it's connections
        /// </summary>
        public virtual void DrawConnections () 
        {
            if (Event.current.type != EventType.Repaint)
                return;
            Vector2 startPos = Knob.GetGUIKnob ().center;
            Vector2 startDir = Knob.GetDirection();
            for (int i = 0; i < Node.OutgoingConnections.Count; i++)
            {
                var conKnob = Node.OutgoingConnections[i].TargetPort.Knob;
                Vector2 endPos = conKnob.GetGUIKnob().center;
                Vector2 endDir = conKnob.GetDirection();
                NodeEditorGUI.DrawConnection(startPos, startDir, endPos, endDir, color);
            }
        }
        
        // public virtual void DrawConnections () 
        // {
        //     if (Event.current.type != EventType.Repaint)
        //         return;
        //     Vector2 startPos = Node.rect.center;
        //     for (int i = 0; i < Connections.Count; i++)
        //     {
        //         //should this be destination?
        //         Vector2 endPos = Connections[i].TargetPort.Node.rect.center;
        //         NodeEditorGUI.DrawConnection (startPos, endPos, ConnectionDrawMethod.StraightLine, color);
        //     }
        // }

        
    }
}