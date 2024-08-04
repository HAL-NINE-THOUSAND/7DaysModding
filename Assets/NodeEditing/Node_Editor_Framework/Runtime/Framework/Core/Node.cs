using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NodeEditorFramework
{
	public abstract partial class Node : ScriptableObject
	{// Host Canvas
		public NodeCanvas canvas;

		public static int debugNextId;
		public int DebugId = debugNextId++;
		
		// Positioning
		public Vector2 position;
		private Vector2 autoSize;
		public Vector2 size { get { return AutoLayout? autoSize : DefaultSize; } }
		public Rect rect { get { return new Rect (position, size); } }
		public Rect fullAABBRect { get { return new Rect(position.x - 20, position.y - 20, size.x + 40, size.y + 40); } }

		// // Dynamic connection ports
		// public List<ConnectionPort> dynamicConnectionPorts = new List<ConnectionPort>();
		// // Static connection ports stored in the actual declaration variables
		// [NonSerialized] public List<ConnectionPort> staticConnectionPorts = new List<ConnectionPort>();
		// // Representative lists of static port declarations aswell as dynamic ports
		// [NonSerialized] public List<ConnectionPort> connectionPorts = new List<ConnectionPort> ();
		// [NonSerialized] public List<ConnectionPort> inputPorts = new List<ConnectionPort> ();
		// [NonSerialized] public List<ConnectionPort> outputPorts = new List<ConnectionPort> ();
		// [NonSerialized] public List<ConnectionKnob> connectionKnobs = new List<ConnectionKnob> ();
		// [NonSerialized] public List<ConnectionKnob> inputKnobs = new List<ConnectionKnob> ();
		// [NonSerialized] public List<ConnectionKnob> outputKnobs = new List<ConnectionKnob> ();

		
		public List<IPort> IncomingPorts { get; private set; }
    public List<IPort> OutgoingPorts { get; private set; }
    public List<Connection> IncomingConnections { get; private set; }
    public List<Connection> OutgoingConnections { get; private set; }

    public Node()
    {
        IncomingPorts = new List<IPort>();
        OutgoingPorts = new List<IPort>();
        IncomingConnections = new List<Connection>();
        OutgoingConnections = new List<Connection>();
    }

    // Add an incoming port to the node
    public void AddIncomingPort(IPort port)
    {
        IncomingPorts.Add(port);
    }

    // Add an outgoing port to the node
    public void AddOutgoingPort(IPort port)
    {
        OutgoingPorts.Add(port);
    }

    public bool ConnectPorts(IPort port)
    {
	    foreach (var p in IncomingPorts)
	    {
		    if (p.IsSamePort(port))
		    {
			    return ConnectToThisNode(port, p);
		    }
	    }

	    return false;
    }
    //
    // public bool RemoveConnection(IPort sourcePort, IPort targetPort)
    // {
	   //  for (var index = 0; index < IncomingConnections.Count; index++)
	   //  {
		  //   var conn = IncomingConnections[index];
    //
		  //   var match = conn.SourcePort == sourcePort && conn.TargetPort == targetPort;
		  //   var wrongWayRound = conn.SourcePort == targetPort && conn.TargetPort == sourcePort;
    //
		  //   if (wrongWayRound)
		  //   {
			 //    Debug.LogWarning("==WRONG WAY ROUND MUPPET!");
		  //   }
		  //   if (match)
		  //   {
			 //    IncomingConnections.RemoveAt(index);
			 //    return true;
		  //   }
	   //  }
	   //  for (var index = 0; index < OutgoingConnections.Count; index++)
	   //  {
		  //   var port = OutgoingConnections[index];
		  //   if (port.TargetPort == sourcePort || port.SourcePort == targetPort)
		  //   {
			 //    OutgoingConnections.RemoveAt(index);
			 //    return true;
		  //   }
	   //  }
    //
	   //  return false;
    // }

    public bool ConnectToThisNode(IPort sourcePort, IPort destinationPort)
    {

	    if (sourcePort.Node == destinationPort.Node)
		    return false;

	    if (sourcePort.Direction != Direction.Out)
		    return false;

	    if (destinationPort.Direction != Direction.In)
		    return false;

	    if (sourcePort.PortType != destinationPort.PortType)
		    return false;

	    var connection = new Connection(sourcePort, destinationPort);
	    sourcePort.Node.OutgoingConnections.Add(connection);
	    IncomingConnections.Add(connection);
	    sourcePort.Connections.Add(connection);
	    destinationPort.Connections.Add(connection);
	    NodeEditor.curNodeCanvas.OnNodeChange(sourcePort.Node);
	    return true;

    }

    // // Connect this node's outgoing port to another node's incoming port if types match
    // public bool ConnectFromOtherNode(Node otherNode, int thisPortIndex, int otherPortIndex)
    // {
    //     if (thisPortIndex < 0 || thisPortIndex >= OutgoingPorts.Count ||
    //         otherPortIndex < 0 || otherPortIndex >= otherNode.IncomingPorts.Count)
    //     {
    //         return false;
    //     }
    //
    //     var thisPort = OutgoingPorts[thisPortIndex];
    //     var otherPort = otherNode.IncomingPorts[otherPortIndex];
    //
    //     if (thisPort.PortType == otherPort.PortType)
    //     {
    //         var connection = new Connection(thisPort, otherPort);
    //         OutgoingConnections.Add(connection);
    //         otherNode.IncomingConnections.Add(connection);
    //         return true;
    //     }
    //
    //     return false;
    // }

    // Get the value of a specific incoming port as its given type
    public T GetIncomingPortValue<T>(int portIndex)
    {
        if (portIndex < 0 || portIndex >= IncomingPorts.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid port index.");
        }

        return IncomingPorts[portIndex].GetValue<T>();
    }

    // Get the value of a specific outgoing port as its given type
    public T GetOutgoingPortValue<T>(int portIndex)
    {
        if (portIndex < 0 || portIndex >= OutgoingPorts.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid port index.");
        }

        return OutgoingPorts[portIndex].GetValue<T>();
    }

    // Get incoming connections of a specific incoming port
    public IEnumerable<Connection> GetIncomingPortConnections(int portIndex)
    {
        if (portIndex < 0 || portIndex >= IncomingPorts.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid port index.");
        }

        var port = IncomingPorts[portIndex];
        foreach (var connection in IncomingConnections)
        {
            if (connection.TargetPort == port)
            {
                yield return connection;
            }
        }
    }

    // Get outgoing connections of a specific outgoing port
    public IEnumerable<Connection> GetOutgoingPortConnections(int portIndex)
    {
        if (portIndex < 0 || portIndex >= OutgoingPorts.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(portIndex), "Invalid port index.");
        }

        var port = OutgoingPorts[portIndex];
        foreach (var connection in OutgoingConnections)
        {
            if (connection.SourcePort == port)
            {
                yield return connection;
            }
        }
    }
		
		// Calculation graph
		[HideInInspector] [NonSerialized]
		public bool calculated = true;

		// Internal
		internal Vector2 contentOffset = Vector2.zero;
		internal Vector2 nodeGUIHeight;
		internal bool ignoreGUIKnobPlacement;
		internal bool isClipped;

		// Style
		public Color backgroundColor = Color.white;


		#region Properties and Settings

		/// <summary>
		/// Gets the ID of the Node
		/// </summary>
		public abstract string GetID { get; }

		/// <summary>
		/// Specifies the node title.
		/// </summary>
		public virtual string Title { get { 
			#if UNITY_EDITOR
				return UnityEditor.ObjectNames.NicifyVariableName (GetID);
			#else
				return name;
			#endif
			} }

		/// <summary>
		/// Specifies the default size of the node when automatic resizing is turned off.
		/// </summary>
		public virtual Vector2 DefaultSize { get { return new Vector2(200, 100); } }

		/// <summary>
		/// Specifies whether the size of this node should be automatically calculated.
		/// If this is overridden to true, MinSize should be set, too.
		/// </summary>
		public virtual bool AutoLayout { get { return false; } }

		/// <summary>
		/// Specifies the minimum size the node can have if no content is present.
		/// </summary>
		public virtual Vector2 MinSize { get { return new Vector2(100, 50); } }

		/// <summary>
		/// Specifies if calculation should continue with the nodes connected to the outputs after the Calculation function returns success
		/// </summary>
		public virtual bool ContinueCalculation { get { return true; } }

		/// <summary>
		/// Specifies whether GUI requires to be updated even when the node is off-screen 
		/// </summary>
		public virtual bool ForceGUIDrawOffScreen { get { return false; } }

		#endregion


		#region Node Implementation

		/// <summary>
		/// Initializes the node with Inputs/Outputs and other data if necessary.
		/// </summary>
		protected virtual void OnCreate() {}
		
		/// <summary>
		/// Draws the Node GUI including all controls and potentially Input/Output labels.
		/// By default, it displays all Input/Output labels.
		/// </summary>
		public virtual void  NodeGUI () 
		{
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();

			for (int i = 0; i < IncomingPorts.Count; i++)
				IncomingPorts[i].Knob.DisplayLayout ();

			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();

			for (int i = 0; i < OutgoingPorts.Count; i++)
				OutgoingPorts[i].Knob.DisplayLayout ();
			
			

			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}

		/// <summary>
		/// Used to display a custom node property editor in the GUI.
		/// By default shows the standard NodeGUI.
		/// </summary>
		public virtual void DrawNodePropertyEditor (bool isEditorWindow = false)
		{
			try
			{ // Draw Node GUI without disturbing knob placement
				ignoreGUIKnobPlacement = true;
				NodeEditorGUI.StartNodeGUI(isEditorWindow);
				GUILayout.BeginVertical(GUI.skin.box);
				NodeGUI();
				GUILayout.EndVertical();
				NodeEditorGUI.EndNodeGUI();
			}
			finally
			{ // Be sure to always reset the state to not mess up other GUI code
				ignoreGUIKnobPlacement = false;
			}
		}
		
		/// <summary>
		/// Calculates the outputs of this Node depending on the inputs.
		/// Returns success
		/// </summary>
		public virtual bool Calculate () { return true; }

		#endregion

		#region Callbacks

		/// <summary>
		/// Callback when the node is deleted
		/// </summary>
		protected internal virtual void OnDelete () {}

		/// <summary>
		/// Callback when the given port on this node was assigned a new connection
		/// </summary>
		protected internal virtual void OnAddConnection (ConnectionPort port, ConnectionPort connection) {}
		
		/// <summary>
		/// Callback when the given port has a connection that was removed.
		/// </summary>
		protected internal virtual void OnRemoveConnection (ConnectionPort port, ConnectionPort connection) {}

		/// <summary>
		/// Should return all additional ScriptableObjects this Node references
		/// </summary>
		public virtual ScriptableObject[] GetScriptableObjects () { return new ScriptableObject[0]; }

		/// <summary>
		/// Replaces all references to any ScriptableObjects this Node holds with the cloned versions in the serialization process.
		/// </summary>
		protected internal virtual void CopyScriptableObjects (System.Func<ScriptableObject, ScriptableObject> replaceSO) {}

		#endregion


		#region General

		/// <summary>
		/// Creates a node of the specified ID at pos on the current canvas, optionally auto-connecting the specified output to a matching input
		/// </summary>
		public static Node Create (string nodeID, Vector2 pos, IPort connectingPort = null, bool silent = false, bool init = true)
		{
			return Create (nodeID, pos, NodeEditor.curNodeCanvas, connectingPort, silent, init);
		}

		/// <summary>
		/// Creates a node of the specified ID at pos on the specified canvas, optionally auto-connecting the specified output to a matching input
		/// silent disables any events, init specifies whether OnCreate should be called
		/// </summary>
		public static Node Create (string nodeID, Vector2 pos, NodeCanvas hostCanvas, IPort connectingPort = null, bool silent = false, bool init = true)
		{
			if (string.IsNullOrEmpty (nodeID) || hostCanvas == null)
				throw new ArgumentException ();
			if (!NodeCanvasManager.CheckCanvasCompability (nodeID, hostCanvas.GetType ()))
				throw new UnityException ("Cannot create Node with ID '" + nodeID + "' as it is not compatible with the current canavs type (" + hostCanvas.GetType ().ToString () + ")!");
			if (!hostCanvas.CanAddNode (nodeID))
				throw new UnityException ("Cannot create Node with ID '" + nodeID + "' on the current canvas of type (" + hostCanvas.GetType ().ToString () + ")!");

			// Create node from data
			NodeTypeData data = NodeTypes.getNodeData (nodeID);
			Node node = (Node)CreateInstance (data.type);
			if(node == null)
				return null;

			// Init node state
			node.canvas = hostCanvas;
			node.name = node.Title;
			node.autoSize = node.DefaultSize;
			node.position = pos;
			ConnectionPortManager.UpdateConnectionPorts (node);
			if (init)
				node.OnCreate();
			
			if (connectingPort != null)
			{
				node.ConnectPorts(connectingPort);
				// // Handle auto-connection and link the output to the first compatible input
				// for (int i = 0; i < node.connectionPorts.Count; i++)
				// {
				// 	if (node.connectionPorts[i].TryApplyConnection (connectingPort, true))
				// 		break;
				// }
			}

			// Add node to host canvas
			hostCanvas.nodes.Add (node);
			if (!silent)
			{ // Callbacks
				hostCanvas.OnNodeChange(connectingPort != null ? connectingPort.Node : node);
				NodeEditorCallbacks.IssueOnAddNode(node);
				hostCanvas.Validate();
				NodeEditor.RepaintClients();
			}

// #if UNITY_EDITOR
// 			if (!silent)
// 			{
// 				List<ConnectionPort> connectedPorts = new List<ConnectionPort>();
// 				foreach (ConnectionPort port in node.connectionPorts)
// 				{ // 'Encode' connected ports in one list (double level cannot be serialized)
// 					foreach (ConnectionPort conn in port.connections)
// 						connectedPorts.Add(conn);
// 					connectedPorts.Add(null);
// 				}
// 				Node createdNode = node;
// 				// Make sure the new node is in the memory dump
// 				NodeEditorUndoActions.CompleteSOMemoryDump(hostCanvas);
// 			}
// #endif

			return node;
		}

		/// <summary>
		/// Deletes this Node from it's host canvas and the save file
		/// </summary>
		public void Delete (bool silent = false) 
		{
			
			if (!canvas.nodes.Contains (this))
				throw new UnityException ("The Node " + name + " does not exist on the Canvas " + canvas.name + "!");
			if (!silent)
				NodeEditorCallbacks.IssueOnDeleteNode (this);

// #if UNITY_EDITOR
// 			if (!silent)
// 			{
// 				List<ConnectionPort> connectedPorts = new List<ConnectionPort>();
// 				foreach (ConnectionPort port in connectionPorts)
// 				{ // 'Encode' connected ports in one list (double level cannot be serialized)
// 					foreach (ConnectionPort conn in port.connections)
// 						connectedPorts.Add(conn);
// 					connectedPorts.Add(null);
// 				}
// 				Node deleteNode = this;
// 				NodeEditorUndoActions.CompleteSOMemoryDump(canvas);
// 			}
// #endif

			canvas.nodes.Remove(this);

			foreach (var conn in IncomingConnections)
			{
				conn.Delete();
			}
			foreach (var conn in OutgoingConnections)
			{
				conn.Delete();
			}

			if (!silent)
				canvas.Validate ();
		}

		#endregion

		#region Drawing

#if UNITY_EDITOR
		/// <summary>
		/// If overridden, the Node can draw to the scene view GUI in the Editor.
		/// </summary>
		public virtual void OnSceneGUI()
		{

		}
#endif

		/// <summary>
		/// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
		/// </summary>
		protected internal virtual void DrawNode () 
		{
			// Create a rect that is adjusted to the editor zoom and pixel perfect
			Rect nodeRect = rect;
			Vector2 pos = NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
			nodeRect.position = new Vector2((int)(nodeRect.x+pos.x), (int)(nodeRect.y+pos.y));
			contentOffset = new Vector2 (0, 20);

			GUI.color = backgroundColor;

			// Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
			Rect headerRect = new Rect (nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
			GUI.color = backgroundColor;
			GUI.Box (headerRect, GUIContent.none);
			GUI.color = Color.white;
			GUI.Label (headerRect, Title, GUI.skin.GetStyle (NodeEditor.curEditorState.selectedNode == this? "labelBoldCentered" : "labelCentered"));

			// Begin the body frame around the NodeGUI
			Rect bodyRect = new Rect (nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);
			GUI.color = backgroundColor;
			GUI.BeginGroup (bodyRect, GUI.skin.box);
			GUI.color = Color.white;
			bodyRect.position = Vector2.zero;
			GUILayout.BeginArea (bodyRect);

			// Call NodeGUI
			GUI.changed = false;

#if UNITY_EDITOR // Record changes done in the GUI function
			UnityEditor.Undo.RecordObject(this, "Node GUI");
#endif
			NodeGUI();
#if UNITY_EDITOR // Make sure it doesn't record anything else after this
			UnityEditor.Undo.FlushUndoRecordObjects();
#endif

			if(Event.current.type == EventType.Repaint)
				nodeGUIHeight = GUILayoutUtility.GetLastRect().max + contentOffset;

			// End NodeGUI frame
			GUILayout.EndArea ();
			GUI.EndGroup ();

			// Automatically node if desired
			AutoLayoutNode ();
		}

		/// <summary>
		/// Resizes the node to either the MinSize or to fit size of the GUILayout in NodeGUI
		/// </summary>
		protected internal virtual void AutoLayoutNode()
		{
			if (!AutoLayout || Event.current.type != EventType.Repaint)
				return;
			
			Rect nodeRect = rect;
			Vector2 size = new Vector2();
			size.y = Math.Max(nodeGUIHeight.y, MinSize.y) + 4;

			// Account for potential knobs that might occupy horizontal space
			float knobSize = 0;
			var verticalKnobs = IncomingPorts.Select(d=> d.Knob).Where (x => x.side == NodeSide.Bottom || x.side == NodeSide.Top);
			if (verticalKnobs.Any())
				knobSize = verticalKnobs.Max ((ConnectionKnob knob) => knob.GetCanvasSpaceKnob ().xMax - nodeRect.xMin);
			size.x = Math.Max (knobSize, Math.Max (nodeGUIHeight.x, MinSize.x));
			
			autoSize = size;
			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Draws the connectionKnobs of this node
		/// </summary>
		protected internal virtual void DrawKnobs () 
		{
			foreach(var p in IncomingPorts)
				p.Knob.DrawKnob();
			foreach(var p in OutgoingPorts)
				p.Knob.DrawKnob();
		}

		/// <summary>
		/// Draws the connections from this node's connectionPorts
		/// </summary>
		protected internal virtual void DrawConnections () 
		{
			if (Event.current.type != EventType.Repaint)
				return;
			for (int i = 0; i < OutgoingPorts.Count; i++)
				OutgoingPorts[i].DrawConnections ();
		}

		#endregion

		#region Node Utility

		/// <summary>
		/// Tests the node whether the specified position is inside any of the node's elements and returns a potentially focused connection knob.
		/// </summary>
		public bool ClickTest(Vector2 position, out ConnectionKnob focusedKnob)
		{
			focusedKnob = null;
			if (rect.Contains(position))
				return true;
			Vector2 dist = position - rect.center;
			if (Math.Abs(dist.x) > size.x || Math.Abs(dist.y) > size.y)
				return false; // Quick check if pos is within double the size
			for (int i = 0; i < IncomingPorts.Count; i++)
			{ // Check if any nodeKnob is focused instead
				if (IncomingPorts[i].Knob.GetCanvasSpaceKnob().Contains(position))
				{
					focusedKnob = IncomingPorts[i].Knob;
					return true;
				}
			}
			for (int i = 0; i < OutgoingPorts.Count; i++)
			{ // Check if any nodeKnob is focused instead
				if (OutgoingPorts[i].Knob.GetCanvasSpaceKnob().Contains(position))
				{
					focusedKnob = OutgoingPorts[i].Knob;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns whether the node acts as an input (no inputs or no inputs assigned)
		/// </summary>
		public bool isInput()
		{
			throw new NotImplementedException("Check this logic, doesn't make sense");
			// for (int i = 0; i < inputPorts.Count; i++)
			// {
			// 	if (inputPorts[i].connected())
			// 		return false;
			// }
			// return true;
		}

		/// <summary>
		/// Returns whether the node acts as an output (no outputs or no outputs assigned)
		/// </summary>
		public bool isOutput()
		{
			
			throw new NotImplementedException("Check this logic, doesn't make sense");
			// for (int i = 0; i < outputPorts.Count; i++)
			// {
			// 	if (outputPorts[i].connected())
			// 		return false;
			// }
			// return true;
		}

		/// <summary>
		/// Returns whether every direct ancestor has been calculated
		/// </summary>
		public bool ancestorsCalculated () 
		{
			for (int i = 0; i < IncomingPorts.Count; i++)
			{
				var port = IncomingPorts[i];
				for (int t = 0; t < port.Connections.Count; t++)
				{
					if (!port.Connections[t].SourcePort.Node.calculated)
						return false;
				}
			}
			return true;
		}
		
		
		
		/// <summary>
		/// Returns whether every direct ancestor has been calculated
		/// </summary>
		public Node getUncalculatedAncestor (Node original) 
		{
			for (int i = 0; i < IncomingPorts.Count; i++)
			{
				var port = IncomingPorts[i];
				for (int t = 0; t < port.Connections.Count; t++)
				{
					var node = port.Connections[t].TargetPort.Node;
					if (node == original)
					{
						Debug.LogWarning("We're in an getUncalculatedAncestor loop!");
						return null;
					}
					if (!node.calculated)
						return node;
				}
			}
			return null;
		}

		/// <summary>
		/// Recursively checks whether this node is a child of the other node
		/// </summary>
		public bool isChildOf (Node otherNode)
		{
			if (otherNode == null || otherNode == this)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			for (int i = 0; i < IncomingPorts.Count; i++)
			{
				var port = IncomingPorts[i];
				for (int t = 0; t < port.Connections.Count; t++)
				{
					Node conBody = port.Connections[t].SourcePort.Node;
					if (conBody == otherNode || conBody.isChildOf(otherNode))
					{
						StopRecursiveSearchLoop();
						return true;
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether this node is in a loop
		/// </summary>
		internal bool isInLoop ()
		{
			if (BeginRecursiveSearchLoop ()) return false;
			for (int i = 0; i < IncomingPorts.Count; i++)
			{
				var port = IncomingPorts[i];
				for (int t = 0; t < port.Connections.Count; t++)
				{
					Node conBody = port.Connections[t].SourcePort.Node;
					if (conBody == startRecursiveSearchNode || conBody.isInLoop())
					{
						StopRecursiveSearchLoop();
						return true;
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// A recursive function to clear all calculations depending on this node.
		/// Usually does not need to be called manually
		/// </summary>
		public void ClearCalculation () 
		{
			calculated = false;
			if (BeginRecursiveSearchLoop ()) return;
			for (int i = 0; i < OutgoingPorts.Count; i++)
			{
				var port = OutgoingPorts[i];
				for (int t = 0; t < port.Connections.Count; t++)
				{
					var conPort = port.Connections[t];
					conPort.TargetPort.Node.ClearCalculation ();
				}
			}
			EndRecursiveSearchLoop ();
		}

		#region Recursive Search Helpers

		[NonSerialized] private static List<Node> recursiveSearchSurpassed = new List<Node> ();
		[NonSerialized] private static Node startRecursiveSearchNode; // Temporary start node for recursive searches

		/// <summary>
		/// Begins the recursive search loop and returns whether this node has already been searched
		/// </summary>
		internal bool BeginRecursiveSearchLoop ()
		{
			if (startRecursiveSearchNode == null) 
			{ // Start search
				if (recursiveSearchSurpassed == null)
					recursiveSearchSurpassed = new List<Node> ();
				recursiveSearchSurpassed.Capacity = canvas.nodes.Count;
				startRecursiveSearchNode = this;
			} 
			// Check and mark node as searched
			if (recursiveSearchSurpassed.Contains (this))
				return true;
			recursiveSearchSurpassed.Add (this);
			return false;
		}

		/// <summary>
		/// Ends the recursive search loop if this was the start node
		/// </summary>
		internal void EndRecursiveSearchLoop () 
		{
			if (startRecursiveSearchNode == this) 
			{ // End search
				recursiveSearchSurpassed.Clear ();
				startRecursiveSearchNode = null;
			}
		}

		/// <summary>
		/// Stops the recursive search loop immediately. Call when you found what you needed.
		/// </summary>
		internal void StopRecursiveSearchLoop () 
		{
			recursiveSearchSurpassed.Clear ();
			startRecursiveSearchNode = null;
		}

		#endregion

		#endregion

		#region Knob Utility

		public ConnectionPort CreateConnectionPort(ConnectionPortAttribute specificationAttribute)
		{
			throw new NotImplementedException("Nope...");
			// ConnectionPort port = specificationAttribute.CreateNew(this);
			// if (port == null)
			// 	return null;
			// dynamicConnectionPorts.Add(port);
			// ConnectionPortManager.UpdateRepresentativePortLists(this);
			// return port;
		}

		public ConnectionKnob CreateConnectionKnob(ConnectionKnobAttribute specificationAttribute)
		{
			return (ConnectionKnob)CreateConnectionPort(specificationAttribute);
		}

		public ValueConnectionKnob CreateValueConnectionKnob(ValueConnectionKnobAttribute specificationAttribute)
		{
			return (ValueConnectionKnob)CreateConnectionPort(specificationAttribute);
		}

		public void DeleteConnectionPort(ConnectionPort dynamicPort)
		{
			dynamicPort.ClearConnections ();
			//dynamicConnectionPorts.Remove(dynamicPort);
			DestroyImmediate(dynamicPort);
			ConnectionPortManager.UpdateRepresentativePortLists(this);
		}

		public void DeleteConnectionPort(int dynamicPortIndex)
		{
			// if (dynamicPortIndex >= 0 && dynamicPortIndex < dynamicConnectionPorts.Count)
			// 	DeleteConnectionPort(dynamicConnectionPorts[dynamicPortIndex]);
		}

		#endregion
	}
}
