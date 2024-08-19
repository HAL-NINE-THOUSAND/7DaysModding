using System;
using System.Collections.Generic;
using NodeEditorFramework.IO;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeEditorFramework
{
	/// <summary>
	///     Central class of NodeEditor providing the GUI to draw the Node Editor Canvas, bundling all other parts of the
	///     Framework
	/// </summary>
	public static class NodeEditor
    {
        public static string editorPath = "Assets/Plugins/Node_Editor_Framework/";

        // The NodeCanvas which represents the currently drawn Node Canvas; globally accessed
        public static NodeCanvas curNodeCanvas;
        public static NodeEditorState curEditorState;

        // GUI callback control
        public static Action NEUpdate;
        public static Action ClientRepaints;

        // Canvas Editing
        private static readonly Stack<NodeCanvas> editCanvasStack = new(4);
        private static readonly Stack<NodeEditorState> editEditorStateStack = new(4);

        // Initiation
        private static bool initiatedBase;
        private static bool initiatedGUI;
        public static bool InitiationError;

        public static void Update()
        {
            if (NEUpdate != null) NEUpdate();
        }

        public static void RepaintClients()
        {
            if (ClientRepaints != null) ClientRepaints();
        }

        #region Setup

        /// <summary>
        ///     Initiates the Node Editor if it wasn't yet
        /// </summary>
        public static void checkInit(bool GUIFunction)
        {
            if (!InitiationError)
            {
                if (!initiatedBase)
                    setupBaseFramework();
                if (GUIFunction && !initiatedGUI)
                    setupGUI();
            }
        }

        /// <summary>
        ///     Resets the initiation state so next time calling checkInit it will re-initiate
        /// </summary>
        public static void resetInit()
        {
            InitiationError = initiatedBase = initiatedGUI = false;
        }

        /// <summary>
        ///     Re-Inits the NodeCanvas regardless of whetehr it was initiated before
        /// </summary>
        public static void ReInit(bool GUIFunction)
        {
            InitiationError = initiatedBase = initiatedGUI = false;

            setupBaseFramework();
            if (GUIFunction)
                setupGUI();
        }

        /// <summary>
        ///     Setup of the base framework. Enough to manage and calculate canvases.
        /// </summary>
        private static void setupBaseFramework()
        {
            CheckEditorPath();

            // Init Resource system. Can be called anywhere else, too, if it's needed before.
            ResourceManager.SetDefaultResourcePath(editorPath + "Runtime/Resources/");

            // Run fetching algorithms searching the script assemblies for Custom Nodes / Connection Types / NodeCanvas Types
            ConnectionPortStyles.FetchConnectionPortStyles();
            NodeTypes.FetchNodeTypes();
            NodeCanvasManager.FetchCanvasTypes();
            ConnectionPortManager.FetchNodeConnectionDeclarations();
            ImportExportManager.FetchIOFormats();

            // Setup Callback system
            NodeEditorCallbacks.SetupReceivers();
            NodeEditorCallbacks.IssueOnEditorStartUp();

            // Init input
            NodeEditorInputSystem.SetupInput();

#if UNITY_EDITOR
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
#endif

            initiatedBase = true;
        }

        /// <summary>
        ///     Setup of the GUI. Only called when a GUI representation is actually used.
        /// </summary>
        private static void setupGUI()
        {
            if (!initiatedBase)
                setupBaseFramework();
            initiatedGUI = false;

            // Init GUIScaleUtility. This fetches reflected calls and might throw a message notifying about incompability.
            GUIScaleUtility.CheckInit();

            if (!NodeEditorGUI.Init())
            {
                InitiationError = true;
                return;
            }

#if UNITY_EDITOR
            RepaintClients();
#endif

            initiatedGUI = true;
        }

        /// <summary>
        ///     Checks the editor path and corrects it when possible.
        /// </summary>
        public static void CheckEditorPath()
        {
#if UNITY_EDITOR
            var script = AssetDatabase.LoadAssetAtPath(editorPath + "Runtime/Framework/NodeEditor.cs", typeof(Object));
            if (script == null)
            {
                // Not installed in default path
                var assets = AssetDatabase.FindAssets("NodeEditorCallbackReceiver"); // Something relatively unique
                if (assets.Length != 1)
                {
                    assets = AssetDatabase.FindAssets("ConnectionPortManager"); // Another try
                    if (assets.Length != 1)
                        throw new UnityException("Node Editor: Not installed in default directory '" + editorPath + "'! Correct path could not be detected! Please correct the editorPath variable in NodeEditor.cs!");
                }

                var correctEditorPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                var subFolderIndex = correctEditorPath.LastIndexOf("Runtime/Framework/");
                if (subFolderIndex == -1)
                    throw new UnityException("Node Editor: Not installed in default directory '" + editorPath + "'! Correct path could not be detected! Please correct the editorPath variable in NodeEditor.cs!");
                correctEditorPath = correctEditorPath.Substring(0, subFolderIndex);

                Debug.LogWarning("Node Editor: Not installed in default directory '" + editorPath + "'! " +
                                 "Editor-only automatic detection adjusted the path to " + correctEditorPath + ", but if you plan to use at runtime, please correct the editorPath variable in NodeEditor.cs!");
                editorPath = correctEditorPath;
            }
#endif
        }

        #endregion

        #region GUI

        /// <summary>
        ///     Draws the Node Canvas on the screen in the rect specified by editorState
        /// </summary>
        public static void DrawCanvas(NodeCanvas nodeCanvas, NodeEditorState editorState)
        {
            if (nodeCanvas == null || editorState == null || !editorState.drawing)
                return;
            checkInit(true);

            DrawSubCanvas(nodeCanvas, editorState);
        }

        public static Action OnEditorClosing;

        /// <summary>
        ///     Draws the Node Canvas on the screen in the rect specified by editorState without one-time wrappers like GUISkin and
        ///     OverlayGUI. Made for nested Canvases (WIP)
        /// </summary>
        private static void DrawSubCanvas(NodeCanvas nodeCanvas, NodeEditorState editorState)
        {
            if (!editorState.drawing)
                return;

            BeginEditingCanvas(nodeCanvas, editorState);
            if (curNodeCanvas == null || curEditorState == null || !curEditorState.drawing)
                return;


            if (Math.Abs(curEditorState.zoom - curEditorState.zoomTarget) > 0.01f)
            {
                var p = 1.0f - (curEditorState.zoomEnds - Time.time) / curEditorState.zoomTime;
                //curEditorState.zoom = Mathf.Lerp(curEditorState.zoomStart, curEditorState.zoomTarget, p);
                curEditorState.zoom = Mathf.MoveTowards(curEditorState.zoom, curEditorState.zoomTarget, curEditorState.totalTime);
                curEditorState.totalTime += Time.deltaTime / 160f;
//				Debug.Log($"Zoom p: {p} | {curEditorState.zoomStart} | {curEditorState.zoomTarget} | {curEditorState.zoom}");
            }
            else
            {
                curEditorState.totalTime = 0;
            }

            //Mathf.MoveTowards(curEditorState.zoom, curEditorState.zoomTarget, 0.008f);

            //var mouseDiffFromCenter = Event.current.mousePosition - editorState.canvasRect.center;  
            //Debug.Log($"Mouse: {mouseDiffFromCenter}| Scaled: {mouseDiffFromCenter * curEditorState.zoom}");

            if (curEditorState.isZoomingIn)
            {
                var zoomDiff = curEditorState.zoomTarget - curEditorState.zoomStart;
                var currDiff = curEditorState.zoomTarget - curEditorState.zoom;

                var p = 1.0f - currDiff / zoomDiff;
                var diff = curEditorState.zoomPanOriginal + (curEditorState.zoomMoveTarget - curEditorState.zoomPanOriginal) * p;
//				Debug.Log($"P: {p} |  Started: {curEditorState.zoomPanOriginal} | Target {curEditorState.zoomMoveTarget}: Current: {diff}");

                if (!float.IsNaN(diff.x) && !float.IsNaN(diff.y))
                    curEditorState.panOffset = diff;
            }

            if (Mathf.Approximately(curEditorState.zoomTarget, curEditorState.zoom))
                //				Debug.Log($"Zoom ended: {curEditorState.zoom}/{curEditorState.zoomTarget}");
                curEditorState.isZoomingIn = false;

            if (Event.current.type == EventType.Repaint)
            {
                // Draw Background when Repainting
                // Offset from origin in tile units
                var tileOffset = new Vector2(-(curEditorState.zoomPos.x * curEditorState.zoom + curEditorState.panOffset.x) / NodeEditorGUI.Background.width,
                    ((curEditorState.zoomPos.y - curEditorState.canvasRect.height) * curEditorState.zoom + curEditorState.panOffset.y) / NodeEditorGUI.Background.height);
                // Amount of tiles
                var tileAmount = new Vector2(Mathf.Round(curEditorState.canvasRect.width * curEditorState.zoom) / NodeEditorGUI.Background.width,
                    Mathf.Round(curEditorState.canvasRect.height * curEditorState.zoom) / NodeEditorGUI.Background.height);
                // Draw tiled background
                GUI.DrawTextureWithTexCoords(curEditorState.canvasRect, NodeEditorGUI.Background, new Rect(tileOffset, tileAmount));
            }

            // Handle input events
            NodeEditorInputSystem.HandleInputEvents(curEditorState);
            if (Event.current.type != EventType.Layout)
                curEditorState.ignoreInput = new List<Rect>();

            // We're using a custom scale method, as default one is messing up clipping rect
            var canvasRect = curEditorState.canvasRect;
            curEditorState.zoomPanAdjust = GUIScaleUtility.BeginScale(ref canvasRect, curEditorState.zoomPos, curEditorState.zoom, NodeEditorGUI.isEditorWindow, false);

            // ---- BEGIN SCALE ----

            // Some features which require zoomed drawing:

            if (curEditorState.navigate)
            {
                // Draw a curve to the origin/active node for orientation purposes
                var startPos = (curEditorState.selectedNode != null ? curEditorState.selectedNode.rect.center : curEditorState.panOffset) + curEditorState.zoomPanAdjust;
                var endPos = Event.current.mousePosition;
                RTEditorGUI.DrawLine(startPos, endPos, Color.green, null, 3);
                RepaintClients();
            }

            if (curEditorState.connectKnob != null)
            {
                // Draw the currently drawn connection
                curEditorState.connectKnob.DrawConnection(Event.current.mousePosition);
                RepaintClients();
            }

            // Draw the groups below everything else
            for (var groupCnt = 0; groupCnt < curNodeCanvas.groups.Count; groupCnt++)
            {
                var group = curNodeCanvas.groups[groupCnt];
                if (Event.current.type == EventType.Layout)
                    group.isClipped = !curEditorState.canvasViewport.Overlaps(group.fullAABBRect);
                if (!group.isClipped)
                    group.DrawGroup();
            }

            // Draw the transitions and connections. Has to be drawn before nodes as transitions originate from node centers
            for (var nodeCnt = 0; nodeCnt < curNodeCanvas.nodes.Count; nodeCnt++)
                if (curNodeCanvas.nodes[nodeCnt] != null)
                    curNodeCanvas.nodes[nodeCnt].DrawConnections();

            if (curNodeCanvas?.nodes?.Count > 0) curNodeCanvas.nodes[0].Rule.Circuit.RunIfDirty();

            // Draw the nodes
            for (var nodeCnt = 0; nodeCnt < curNodeCanvas.nodes.Count; nodeCnt++)
            {
                var node = curNodeCanvas.nodes[nodeCnt];
                if (node == null) continue;
                if (Event.current.type == EventType.Layout)
                    node.isClipped = !curEditorState.canvasViewport.Overlaps(node.fullAABBRect);
                if (!node.isClipped || node.ForceGUIDrawOffScreen)
                {
                    node.DrawNode();
                    if (Event.current.type == EventType.Repaint)
                        node.DrawKnobs();
                }
            }

            // ---- END SCALE ----

            // End scaling group
            GUIScaleUtility.EndScale();

            // Handle input events with less priority than node GUI controls
            NodeEditorInputSystem.HandleLateInputEvents(curEditorState);

            EndEditingCanvas();
        }

        /// <summary>
        ///     Sets the specified canvas as the current context most functions work on
        /// </summary>
        public static void BeginEditingCanvas(NodeCanvas canvas)
        {
            var state = canvas.editorStates.Length >= 1 ? canvas.editorStates[0] : null;
            BeginEditingCanvas(canvas, state);
        }

        /// <summary>
        ///     Sets the specified canvas as the current context most functions work on
        /// </summary>
        public static void BeginEditingCanvas(NodeCanvas canvas, NodeEditorState state)
        {
            if (state != null && state.canvas != canvas)
                state = null; // State does not belong to the canvas

            editCanvasStack.Push(canvas);
            editEditorStateStack.Push(state);
            curNodeCanvas = canvas;
            curEditorState = state;
        }

        /// <summary>
        ///     Restores the previously edited canvas as the current context
        /// </summary>
        public static void EndEditingCanvas()
        {
            curNodeCanvas = editCanvasStack.Pop();
            curEditorState = editEditorStateStack.Pop();
        }

        #endregion

        #region Space Transformations

        /// <summary>
        ///     Returns the node at the specified canvas-space position in the current editor
        /// </summary>
        public static Node NodeAtPosition(Vector2 canvasPos)
        {
            ConnectionKnob focusedKnob;
            return NodeAtPosition(curEditorState, canvasPos, out focusedKnob);
        }

        /// <summary>
        ///     Returns the node at the specified canvas-space position in the current editor and returns a possible focused knob
        ///     aswell
        /// </summary>
        public static Node NodeAtPosition(Vector2 canvasPos, out ConnectionKnob focusedKnob)
        {
            return NodeAtPosition(curEditorState, canvasPos, out focusedKnob);
        }

        /// <summary>
        ///     Returns the node at the specified canvas-space position in the specified editor and returns a possible focused knob
        ///     aswell
        /// </summary>
        public static Node NodeAtPosition(NodeEditorState editorState, Vector2 canvasPos, out ConnectionKnob focusedKnob)
        {
            focusedKnob = null;
            if (editorState == null || NodeEditorInputSystem.shouldIgnoreInput(editorState))
                return null;
            var canvas = editorState.canvas;
            for (var nodeCnt = canvas.nodes.Count - 1; nodeCnt >= 0; nodeCnt--)
            {
                // Check from top to bottom because of the render order
                var node = canvas.nodes[nodeCnt];
                if (node.ClickTest(canvasPos, out focusedKnob))
                    return node; // Node is clicked on
            }

            return null;
        }

        /// <summary>
        ///     Transforms screen space elements in the current editor into canvas space (Level of Nodes, ...)
        /// </summary>
        public static Vector2 ScreenToCanvasSpace(Vector2 screenPos)
        {
            return ScreenToCanvasSpace(curEditorState, screenPos);
        }

        /// <summary>
        ///     Transforms screen space elements in the specified editor into canvas space (Level of Nodes, ...)
        /// </summary>
        public static Vector2 ScreenToCanvasSpace(NodeEditorState editorState, Vector2 screenPos)
        {
            return (screenPos - editorState.canvasRect.position - editorState.zoomPos) * editorState.zoom - editorState.panOffset;
        }

        #endregion
    }
}