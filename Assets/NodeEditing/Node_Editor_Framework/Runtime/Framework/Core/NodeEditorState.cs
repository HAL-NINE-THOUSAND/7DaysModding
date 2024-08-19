using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework
{
    public partial class NodeEditorState : ScriptableObject
    {
        // holds the state of a NodeCanvas inside a NodeEditor
        public NodeCanvas canvas;
        public NodeEditorState parentEditor;

        // Selection State
        public Node selectedNode; // selected Node

        // Navigation State
        public Vector2 panOffset; // pan offset
        public float zoom = 1; // zoom; Ranges in 0.2er-steps from 0.6-2.0; applied 1/zoom;
        public float zoomTarget = 1; // zoom; Ranges in 0.2er-steps from 0.6-2.0; applied 1/zoom;
        public float zoomEnds;
        public float zoomTime = 0.5f;
        public float totalTime;
        [NonSerialized] public NodeGroup activeGroup; // NodeGroup that is currently interacted with

        [NonSerialized] public Rect canvasRect; // canvas rect in GUI space

        // Current Action
        [NonSerialized] public ConnectionKnob connectKnob; // connection this output
        [NonSerialized] public bool dragNode; // node dragging

        // Canvas options
        [NonSerialized] public bool drawing = true; // whether to draw the canvas
        [NonSerialized] public ConnectionKnob focusedConnectionKnob; // ConnectionKnob under mouse
        [NonSerialized] public Node focusedNode; // Node under mouse
        [NonSerialized] public List<Rect> ignoreInput = new(); // Rects inside the canvas to ignore input in (nested canvases, fE)
        [NonSerialized] public bool navigate; // navigation ('N')
        [NonSerialized] public bool panWindow; // window panning
        [NonSerialized] public bool resizeGroup; // whether the active group is being resized; if not, it is dragged
        [NonSerialized] public Vector2 zoomPanAdjust; // calculated value to offset elements with when zooming

        // Temporary variables
        public Vector2 zoomPos => canvasRect.size / 2; // zoom center in canvas space
        public Rect canvasViewport => new(-panOffset - zoomPos * zoom, canvasRect.size * zoom); // canvas viewport in canvas space (same as nodes)

        public void StopZoom()
        {
            zoomTarget = zoom;
            isZoomingIn = false;
        }

        #region DragHelper

        [NonSerialized] public Vector2 zoomMoveTarget; // drag start position (mouse)
        [NonSerialized] public Vector2 zoomOutMouseStart; // drag start position (mouse)
        [NonSerialized] public Vector2 zoomPanOriginal; // drag start position (mouse)
        [NonSerialized] public bool isZoomingIn; // drag start position (mouse)
        [NonSerialized] public float zoomStart; // drag start position (mouse)


        [NonSerialized] public string dragUserID; // dragging source
        [NonSerialized] public Vector2 dragMouseStart; // drag start position (mouse)
        [NonSerialized] public Vector2 dragObjectStart; // start position of the dragged object
        [NonSerialized] public Vector2 dragOffset; // offset for both node dragging and window panning
        public Vector2 dragObjectPos => dragObjectStart + dragOffset; // position of the dragged object

        /// <summary>
        ///     Starts a drag operation with the given userID and initial mouse and object position
        ///     Returns false when a different user already claims this drag operation
        /// </summary>
        public bool StartDrag(string userID, Vector2 mousePos, Vector2 objectPos)
        {
            if (!string.IsNullOrEmpty(dragUserID) && dragUserID != userID)
                return false;
            dragUserID = userID;
            dragMouseStart = mousePos;
            dragObjectStart = objectPos;
            dragOffset = Vector2.zero;
            StopZoom();
            return true;
        }

        /// <summary>
        ///     Updates the current drag with the passed new mouse position and returns the drag offset change since the last
        ///     update
        /// </summary>
        public Vector2 UpdateDrag(string userID, Vector2 newDragPos)
        {
            if (dragUserID != userID)
                throw new UnityException("User ID " + userID + " tries to interrupt drag from " + dragUserID);
            var prevOffset = dragOffset;
            dragOffset = (newDragPos - dragMouseStart) * zoom;
            return dragOffset - prevOffset;
        }

        /// <summary>
        ///     Ends the drag of the specified userID
        /// </summary>
        public Vector2 EndDrag(string userID)
        {
            if (dragUserID != userID)
                throw new UnityException("User ID " + userID + " tries to end drag from " + dragUserID);
            var dragPos = dragObjectPos;
            dragUserID = "";
            dragOffset = dragMouseStart = dragObjectStart = Vector2.zero;
            return dragPos;
        }

        #endregion
    }
}