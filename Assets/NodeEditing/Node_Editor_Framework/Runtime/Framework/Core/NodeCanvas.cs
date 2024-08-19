using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditing.Node_Editor_Framework.Runtime;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Core;
using UnityEngine;

namespace NodeEditorFramework
{
	/// <summary>
	///     Base class for all canvas types
	/// </summary>
	public abstract class NodeCanvas : ScriptableObject
    {
        public NodeEditorState[] editorStates = new NodeEditorState[0];

        public string saveName;
        public string savePath;
        public bool livesInScene;

        public List<Node> nodes = new();
        public List<NodeGroup> groups = new();

        public MessageArea Messages;

        public INodeCanvasParent parent;

        [NonSerialized] public List<ScriptableObject> SOMemoryDump = new();

        public NodeCanvasTraversal Traversal;
        public virtual string canvasName => "DEFAULT";

        public virtual bool allowSceneSaveOnly => false;
        public virtual bool allowRecursion => false;

        public Circuit Circuit { get; set; }


        public void SetCircuit(Circuit circuit, INodeCanvasParent parentGameObject)
        {
            parent = parentGameObject;
            LoadCircuit(circuit);
        }

        public void LoadCircuit(Circuit circuit)
        {
            Messages ??= new MessageArea(this);
            Circuit = circuit;
            Circuit.OnRun = LogRun;

            nodes.Clear();
            foreach (var rule in circuit.Rules.Values) Node.Create(rule, this);

            foreach (var connection in circuit.Connections)
            {
                var outputNode = nodes.FirstOrDefault(d => d.Rule.RuleId == connection.Value);

                if (outputNode == null)
                    continue;

                var inputNode = nodes.FirstOrDefault(d => d.Rule.Inputs.Any(d => d.InputId == connection.Key));

                var inputKnob = inputNode.IncomingKnobs.First(d => d.InputId == connection.Key);
                var sourceInputRule = inputKnob.node.Rule;
                var sourceInput = inputKnob.Port;

                var outputKnob = outputNode.OutgoingKnobs.First();
                ;
                var targetRule = outputKnob.node.Rule;
                outputKnob.connectedKnobs.Add(inputKnob);
            }

            circuit.Run();
        }

        private void LogRun(int totalMilliseconds, int outNodeCount)
        {
            AddMessage($"Circuit took {totalMilliseconds}ms for {outNodeCount} out nodes");
        }


        public Node GetNodeForRule(IRule rule)
        {
            foreach (var node in nodes)
                if (node.Rule == rule)
                    return node;
            return null;
        }

        #region Constructors

        /// <summary>
        ///     Creates a canvas of the specified generic type
        /// </summary>
        public static T CreateCanvas<T>() where T : NodeCanvas
        {
            if (typeof(T) == typeof(NodeCanvas))
                throw new Exception("Cannot create canvas of type 'NodeCanvas' as that is only the base class. Please specify a valid subclass!");
            var canvas = CreateInstance<T>();
            canvas.name = canvas.saveName = "New " + canvas.canvasName;

            NodeEditor.BeginEditingCanvas(canvas);
            canvas.OnCreate();
            NodeEditor.EndEditingCanvas();
            return canvas;
        }

        /// <summary>
        ///     Creates a canvas of the specified canvasType as long as it is a subclass of NodeCanvas
        /// </summary>
        public static NodeCanvas CreateCanvas(Type canvasType)
        {
            NodeCanvas canvas;
            if (canvasType != null && canvasType.IsSubclassOf(typeof(NodeCanvas)))
                canvas = CreateInstance(canvasType) as NodeCanvas;
            else
                return null;
            canvas.name = canvas.saveName = "New " + canvas.canvasName;

            NodeEditor.BeginEditingCanvas(canvas);
            canvas.OnCreate();
            NodeEditor.EndEditingCanvas();
            return canvas;
        }

        #endregion

        #region Extension Methods

        // GENERAL

        protected virtual void OnCreate()
        {
        }

        protected virtual void ValidateSelf()
        {
        }

        public virtual void OnBeforeSavingCanvas()
        {
        }

        public virtual bool CanAddNode(string nodeID)
        {
            return true;
        }

        // GUI

        public virtual void DrawCanvasPropertyEditor()
        {
        }

        // ADDITIONAL SERIALIZATION

        /// <summary>
        ///     Should return all additional ScriptableObjects this Node references
        /// </summary>
        public virtual ScriptableObject[] GetScriptableObjects()
        {
            return new ScriptableObject[0];
        }

        /// <summary>
        ///     Replaces all references to any ScriptableObjects this Node holds with the cloned versions in the serialization
        ///     process.
        /// </summary>
        protected internal virtual void CopyScriptableObjects(Func<ScriptableObject, ScriptableObject> replaceSO)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Trigger traversal of the whole canvas
        /// </summary>
        public void TraverseAll()
        {
            if (Traversal != null)
                Traversal.TraverseAll();
        }

        /// <summary>
        ///     Specifies a node change, usually triggering traversal from that node
        /// </summary>
        public void OnNodeChange(Node node)
        {
            if (Traversal != null && node != null)
                Traversal.OnChange(node);
        }

        public void AddMessage(string msg)
        {
            Messages.AddMessage(msg);
        }

        /// <summary>
        ///     Validates this canvas, checking for any broken nodes or references and cleans them.
        /// </summary>
        public bool Validate(bool repair = true)
        {
            Messages ??= new MessageArea(this);

            NodeEditor.checkInit(false);

            // Check Groups
            if (!CheckNodeCanvasList(ref groups, "groups", repair) && !repair) return false;

            // Check Nodes
            if (!CheckNodeCanvasList(ref nodes, "nodes", repair) && !repair) return false;

            // Check Connection ports
            foreach (var node in nodes)
            {
                ConnectionPortManager.UpdateConnectionPorts(node);
                if (node.canvas != this && !repair) return false;
                node.canvas = this;

                //Debug.LogWarning("VALIDATE ME!");
                // foreach (var port in node.IncomingPorts)
                // 	if (!port.Validate(node, repair) && !repair) return false;
            }

            // Check EditorStates
            if (editorStates == null)
                editorStates = new NodeEditorState[0];
            editorStates = editorStates.Where(state => state != null).ToArray();
            foreach (var state in editorStates)
                if (!nodes.Contains(state.selectedNode))
                    state.selectedNode = null;

            // Validate CanvasType-specific stuff
            ValidateSelf();

            return true;
        }

        /// <summary>
        ///     Checks the specified list and assures it is initialized, contains no null nodes and it it does, removes them and
        ///     outputs an error.
        /// </summary>
        private bool CheckNodeCanvasList<T>(ref List<T> list, string listName, bool repair)
        {
            if (list == null)
            {
                Debug.LogWarning("NodeCanvas '" + name + "' " + listName + " were erased and set to null! Automatically fixed!");
                list = new List<T>();
            }

            var originalCount = list.Count;
            for (var i = 0; i < list.Count; i++)
                if (list[i] == null)
                {
                    if (!repair) return false;
                    list.RemoveAt(i);
                    i--;
                }

            if (originalCount != list.Count)
                Debug.LogWarning("NodeCanvas '" + name + "' contained " + (originalCount - list.Count) + " broken (null) " + listName + "! Automatically fixed!");
            return originalCount == list.Count;
        }

        /// <summary>
        ///     Updates the source of this canvas to the specified path, updating saveName and savePath aswell as livesInScene when
        ///     prefixed with "SCENE/"
        /// </summary>
        public void UpdateSource(string path)
        {
            if (path == savePath)
                return;
            string newName;
            if (path.StartsWith("SCENE/"))
            {
                newName = path.Substring(6);
            }
            else
            {
                var nameStart = Mathf.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1;
                newName = path.Substring(nameStart, path.Length - nameStart - 6);
            }

            if (!newName.ToLower().Contains("lastsession") && !newName.ToLower().Contains("cursession"))
            {
                savePath = path;
                saveName = newName;
                livesInScene = path.StartsWith("SCENE/");
            }
        }

        #endregion
    }
}