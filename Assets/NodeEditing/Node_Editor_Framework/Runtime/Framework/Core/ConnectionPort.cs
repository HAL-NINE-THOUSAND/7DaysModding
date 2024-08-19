﻿using System;
using System.Collections.Generic;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace NodeEditorFramework
{
    public enum NodeSide : byte
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    public enum Direction
    {
        None,
        In,
        Out
    }

    public enum ConnectionShape
    {
        Line,
        Bezier
    }

    public enum ConnectionCount
    {
        Single,
        Multi,
        Max
    }

    [Serializable]
    public class ConnectionPort : ScriptableObject
    {
        // Properties
        [FormerlySerializedAs("body")] public Node node;
        public Direction direction = Direction.None;
        public ConnectionCount maxConnectionCount = ConnectionCount.Multi;

        // Connection Style
        public string styleID;
        protected ConnectionPortStyle _connectionStyle;

        [NonSerialized] public Color color = Color.white;

        public IPort Port;
        public IRule rule;
        public virtual ConnectionShape shape => ConnectionShape.Line;

        protected ConnectionPortStyle ConnectionStyle
        {
            get
            {
                CheckConnectionStyle();
                return _connectionStyle;
            }
        }

        protected virtual Type styleBaseClass => typeof(ConnectionPortStyle);

        public void OnEnable()
        {
        }
        // // Connections
        // [SerializeField]
        // protected List<ConnectionPort> _connections = new List<ConnectionPort> ();
        // public List<ConnectionPort> connections { get { return _connections; } }

        public void Init(IPort port, Node node, string knobName)
        {
            Port = port;

            this.node = node;
            name = knobName;
        }

        public bool Validate(Node nodeBody, bool repair = true)
        {
            if (node == null || node != nodeBody)
            {
                Debug.LogError("Port " + name + " has no node body assigned! Fixed!");
                if (!repair) return false;
                node = nodeBody;
            }

            // if (_connections == null)
            // {
            // 	if (!repair) return false;
            // 	_connections = new List<ConnectionPort>();
            // }
            // int originalCount = _connections.Count;
            // for (int i = 0; i < _connections.Count; i++)
            // {
            // 	if (_connections[i] == null)
            // 	{
            // 		if (!repair) return false;
            // 		_connections.RemoveAt(i);
            // 		i--;
            // 	}
            // }
            // if (originalCount != _connections.Count)
            // 	Debug.LogWarning("Removed " + (originalCount - _connections.Count) + " broken (null) connections from node " + body.name + "! Automatically fixed!");
            // return originalCount == _connections.Count;
            return true;
        }

        public virtual IEnumerable<string> AdditionalDynamicKnobData()
        {
            return new List<string> { "styleID", "direction", "maxConnectionCount" };
        }

        #region Connection GUI

        /// <summary>
        ///     Checks and fetches the connection style declaration specified by the styleID
        /// </summary>
        protected void CheckConnectionStyle()
        {
            if (_connectionStyle == null || !_connectionStyle.isValid())
            {
                _connectionStyle = ConnectionPortStyles.GetPortStyle(styleID, styleBaseClass);
                if (_connectionStyle == null || !_connectionStyle.isValid())
                    color = NodeEditorGUI.RandomColorHSV(styleID.GetHashCode(), 0, 1, 0.6f, 0.8f, 0.8f, 1.4f);
                else
                    color = _connectionStyle.Color;
            }
        }

        /// <summary>
        ///     Draws the connection lines from this port to all it's connections
        /// </summary>
        public virtual void DrawConnections()
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var startPos = node.rect.center;
            Debug.LogWarning("draw here connections");
            // for (int i = 0; i < Port.Connections.Count; i++)
            // {
            // 	//should this be destination?
            // 	Vector2 endPos = Port.Connections[i].TargetPort.Node.rect.center;
            // 	NodeEditorGUI.DrawConnection (startPos, endPos, ConnectionDrawMethod.StraightLine, color);
            // }
        }

        /// <summary>
        ///     Draws a connection line from the current knob to the specified position
        /// </summary>
        public virtual void DrawConnection(Vector2 endPos)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            NodeEditorGUI.DrawConnection(node.rect.center, endPos, ConnectionDrawMethod.StraightLine, color);
        }

        #endregion

        #region Connection Utility

        /// <summary>
        ///     Returns whether this port is connected to any other port
        /// </summary>
        public bool connected()
        {
            var ret = node.Rule.Circuit.IsInputConnected(Port.InputId);
            return ret;
        }

        /// <summary>
        ///     Returns the connection with the specified index or null if not existant
        /// </summary>
        public object connection(int index)
        {
            Debug.LogWarning("connection index");
            return null;
            // if (Port.Connections.Count <= index)
            // 	throw new IndexOutOfRangeException ("connections[" + index + "] of '" + name + "'");
            // return Port.Connections[index];
        }

        /// <summary>
        ///     Tries to apply a connection between this port and the specified port
        /// </summary>
        public bool TryApplyConnection(ConnectionPort port, bool silent = false)
        {
            throw new NotImplementedException("check here");
            // if (CanApplyConnection (port)) 
            // { // This port and the specified port can be connected
            // 	ApplyConnection (port, silent);
            // 	return true;
            // }
            // return false;
        }

        /// <summary>
        ///     Determines whether a connection can be applied between this port and the specified port
        /// </summary>
        public virtual bool CanApplyConnection(ConnectionPort port)
        {
            throw new NotImplementedException("check here");
            // if (port == null || body == port.body || connections.Contains (port) || body.canvas != port.body.canvas)
            // 	return false;
            //
            // if (direction == Direction.None && port.direction == Direction.None)
            // 	return true; // None-Directive connections can always connect
            //
            // if (direction == Direction.In && port.direction != Direction.Out)
            // 	return false; // Cannot connect inputs with anything other than outputs
            // if (direction == Direction.Out && port.direction != Direction.In)
            // 	return false; // Cannot connect outputs with anything other than inputs
            //
            // if (!body.canvas.allowRecursion)
            // { // Assure no loop would be created
            // 	bool loop;
            // 	if (direction == Direction.Out) loop = body.isChildOf (port.body);
            // 	else 							loop = port.body.isChildOf (body);
            // 	if (loop)
            // 	{ // Loop would be created, not allowed
            // 		Debug.LogWarning ("Cannot apply connection: Recursion detected!");
            // 		return false;
            // 	}
            // }
            //
            // return true;
        }

        /// <summary>
        ///     Applies a connection between between this port and the specified port.
        ///     'CanApplyConnection' has to be checked before to avoid interferences!
        /// </summary>
        public void ApplyConnection(ConnectionPort port, bool silent = false)
        {
            throw new NotImplementedException("check here");

            // if (port == null) 
            // 	return;
            //
            // if (maxConnectionCount == ConnectionCount.Single && connections.Count > 0)
            // { // Respect maximum connection count on this port
            // 	RemoveConnection(connections[0], silent);
            // 	connections.Clear ();
            // }
            // connections.Add(port);
            //
            // if (port.maxConnectionCount == ConnectionCount.Single && port.connections.Count > 0)
            // { // Respect maximum connection count on the other port
            // 	port.RemoveConnection(port.connections[0], silent);
            // 	port.connections.Clear ();
            // }
            // port.connections.Add (this);
            //
            // if (!silent)
            // { // Callbacks
            // 	port.body.OnAddConnection (port, this);
            // 	body.OnAddConnection (this, port);
            // 	NodeEditorCallbacks.IssueOnAddConnection (this, port);
            // 	body.canvas.OnNodeChange(direction == Direction.In? port.body : body);
            // }
        }

        /// <summary>
        ///     Clears all connections of this port to other ports
        /// </summary>
        public void ClearConnections(bool silent = false)
        {
            throw new NotImplementedException("check here");
            // while (connections.Count > 0)
            // 	RemoveConnection (connections[0], silent);
        }

        /// <summary>
        ///     Removes the connection of this port to the specified port if existant
        /// </summary>
        public void RemoveConnection(ConnectionPort port, bool silent = false)
        {
            throw new NotImplementedException("check here");

            // if (port == null)
            // {
            // 	Debug.LogWarning("Cannot delete null port!");
            // 	//connections.RemoveAll (p => p != null);
            // 	return;
            // }
            //
            // if (!silent)
            // {
            // 	port.body.OnRemoveConnection(port, this);
            // 	this.body.OnRemoveConnection(this, port);
            // 	NodeEditorCallbacks.IssueOnRemoveConnection (this, port);
            // }
            //
            // port.connections.Remove (this);
            // connections.Remove (port);
            //
            // if (!silent) body.canvas.OnNodeChange (body);
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConnectionPortAttribute : Attribute
    {
        public Direction Direction;
        public ConnectionCount MaxConnectionCount;
        public string Name;
        public string StyleID;

        public ConnectionPortAttribute(string name)
        {
            Setup(name, Direction.None, "Default", ConnectionCount.Multi);
        }

        public ConnectionPortAttribute(string name, Direction direction)
        {
            Setup(name, direction, "Default", ConnectionCount.Multi);
        }

        public ConnectionPortAttribute(string name, Direction direction, ConnectionCount maxCount)
        {
            Setup(name, direction, "Default", maxCount);
        }

        public ConnectionPortAttribute(string name, string styleID)
        {
            Setup(name, Direction.None, styleID, ConnectionCount.Multi);
        }

        public ConnectionPortAttribute(string name, Direction direction, string styleID)
        {
            Setup(name, direction, styleID, ConnectionCount.Multi);
        }

        public ConnectionPortAttribute(string name, Direction direction, string styleID, ConnectionCount maxCount)
        {
            Setup(name, direction, styleID, maxCount);
        }

        public virtual Type ConnectionType => typeof(ConnectionPort);

        private void Setup(string name, Direction direction, string styleID, ConnectionCount maxCount)
        {
            Name = name;
            Direction = direction;
            StyleID = styleID;
            MaxConnectionCount = maxCount;
        }

        public bool MatchFieldType(Type fieldType)
        {
            return fieldType.IsAssignableFrom(ConnectionType);
        }

        public virtual bool IsCompatibleWith(ConnectionPort port)
        {
            if (port.GetType() != ConnectionType)
                return false;
            if (port.styleID != StyleID)
                return false;
            if (!(Direction == Direction.None && port.direction == Direction.None)
                && !(Direction == Direction.In && port.direction == Direction.Out)
                && !(Direction == Direction.Out && port.direction == Direction.In))
                return false;
            return true;
        }

        public virtual ConnectionPort CreateNew(IPort port, Node node)
        {
            throw new NotImplementedException("Nope...");
            // ConnectionPort port = ScriptableObject.CreateInstance<ConnectionPort> ();
            // port.Init (body, Name);
            // port.direction = Direction;
            // port.styleID = StyleID;
            // port.maxConnectionCount = MaxConnectionCount;
            // return port;
        }

        public virtual void UpdateProperties(ConnectionPort port)
        {
            port.name = Name;
            port.direction = Direction;
            port.styleID = StyleID;
            port.maxConnectionCount = MaxConnectionCount;
        }
    }

    [ReflectionUtility.ReflectionSearchIgnore]
    public class ConnectionPortStyle
    {
        protected Color color;
        protected string identifier;

        public ConnectionPortStyle()
        {
        }

        public ConnectionPortStyle(string name)
        {
            identifier = name;
            GenerateColor();
        }

        public virtual string Identifier => identifier;
        public virtual Color Color => color;

        public void GenerateColor()
        {
            // Generate consistent color for a type - using string because it delivers greater variety of colors than type hashcode
            color = NodeEditorGUI.RandomColorHSV(Identifier.GetHashCode(), 0, 1, 0.6f, 0.8f, 0.8f, 1.4f);
        }

        public virtual bool isValid()
        {
            return true;
        }
    }
}