using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework
{
    [Serializable]
    public class ConnectionKnob : ConnectionPort
    {
        public float sidePosition; // Position on the side, top->bottom, left->right
        public float sideOffset; // Offset from the side

        public List<ConnectionKnob> connectedKnobs = new();

        public Guid InputId;

        // Knob GUI
        protected Texture2D knobTexture;

        private NodeSide noruleSide = NodeSide.Left;
        private NodeSide noruleSideOut = NodeSide.Right;

        // Properties
        public override ConnectionShape shape => ConnectionShape.Bezier;
        // Connections

        //new public List<ConnectionKnob> connections { get { return _connections.OfType<ConnectionKnob> ().ToList (); } }

        // Knob Style
        protected override Type styleBaseClass => typeof(ConnectionKnobStyle);

        protected new ConnectionKnobStyle ConnectionStyle
        {
            get
            {
                CheckConnectionStyle();
                return (ConnectionKnobStyle)_connectionStyle;
            }
        }

        private float knobAspect => knobTexture != null ? (float)knobTexture.width / knobTexture.height : 1;
        private GUIStyle labelStyle => GUI.skin.GetStyle(inputSide == NodeSide.Right ? "labelRight" : "labelLeft");

        // Knob Position
        private NodeSide defaultSide => direction == Direction.Out ? NodeSide.Right : NodeSide.Left;

        public NodeSide inputSide
        {
            get => rule?.InputPosition ?? noruleSide;
            set
            {
                if (rule != null)
                    rule.InputPosition = value;
                else
                    noruleSide = value;
            }
        }

        public NodeSide outputSide
        {
            get => rule?.OutputPosition ?? noruleSideOut;
            set
            {
                if (rule != null)
                    rule.OutputPosition = value;
                else
                    noruleSideOut = value;
            }
        }

        public void Init(IPort port, Node node, string name, Direction dir)
        {
            base.Init(port, node, name);
            direction = dir;
            maxConnectionCount = dir == Direction.In ? ConnectionCount.Single : ConnectionCount.Multi;
            //inputSide = dir == Direction.Out? NodeSide.Right : NodeSide.Left;
            sidePosition = 0;
        }

        public void Init(IPort port, Node node, string name, Direction dir, NodeSide nodeSide, float nodeSidePosition = 0)
        {
            base.Init(port, node, name);
            direction = dir;
            maxConnectionCount = dir == Direction.In ? ConnectionCount.Single : ConnectionCount.Multi;
            //inputSide = nodeSide;
            sidePosition = nodeSidePosition;
        }

        // new public ConnectionKnob connection (int index) 
        // {
        // 	if (connections.Count <= index)
        // 		throw new IndexOutOfRangeException ("connections[" + index + "] of '" + name + "'");
        // 	return connections[index];
        // }

        public override IEnumerable<string> AdditionalDynamicKnobData()
        {
            return base.AdditionalDynamicKnobData().Concat(new List<string> { "side", "sidePosition", "sideOffset" });
        }

        #region Knob Texture

        /// <summary>
        ///     Checks the texture and requests to load it again if necessary
        /// </summary>
        internal void CheckKnobTexture()
        {
            // if (inputSide == 0)
            // 	inputSide = defaultSide;
            if (knobTexture == null)
                UpdateKnobTexture();
        }

        /// <summary>
        ///     Requests to reload the knobTexture and adapts it to the position and orientation
        /// </summary>
        public void UpdateKnobTexture()
        {
            ReloadTexture();
            if (knobTexture == null)
                throw new UnityException("Knob texture of " + name + " could not be loaded!");

            var rotationSteps = direction == Direction.In ? (byte)inputSide : (byte)outputSide + 2; // getRotationStepsAntiCW (defaultSide, side);

            if (rotationSteps != 0)
            {
                // Rotate Knob texture according to the side it's used on
                string[] mods = { "Rotation:" + rotationSteps };
                Texture2D modKnobTex = null;

                // Try to get standard texture in memory
                var memoryTex = ResourceManager.FindInMemory(knobTexture);
                if (memoryTex != null)
                {
                    // Texture does exist in memory, so built a mod including rotation and try to find it again
                    mods = ResourceManager.AppendMod(memoryTex.modifications, "Rotation:" + rotationSteps);
                    ResourceManager.TryGetTexture(memoryTex.path, ref modKnobTex, mods);
                }

                if (modKnobTex == null)
                {
                    // Rotated version does not exist yet, so create and record it
                    modKnobTex = RTEditorGUI.RotateTextureCW(knobTexture, rotationSteps);
                    ResourceManager.AddTextureToMemory(memoryTex.path, modKnobTex, mods);
                }

                knobTexture = modKnobTex;
            }
        }

        /// <summary>
        ///     Requests to reload the source knob texture
        /// </summary>
        protected virtual void ReloadTexture()
        {
//			knobTexture = RTEditorGUI.ColorToTex (1, Color.red);
//			knobTexture = ResourceManager.GetTintedTexture (direction == Direction.Out? "Textures/Out_Knob.png" : "Textures/In_Knob.png", color);
            knobTexture = ConnectionStyle == null ? Texture2D.whiteTexture : ConnectionStyle.InKnobTex; // (direction == Direction.Out? ConnectionStyle.OutKnobTex : ConnectionStyle.InKnobTex);
        }

        #endregion

        #region Knob Position

        /// <summary>
        ///     Gets the Knob rect in GUI space, NOT ZOOMED
        /// </summary>
        public Rect GetGUIKnob()
        {
            var rect = GetCanvasSpaceKnob();
            rect.position += NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
            return rect;
        }

        /// <summary>
        ///     Get the Knob rect in screen space, ZOOMED, for Input detection purposes
        /// </summary>
        public Rect GetCanvasSpaceKnob()
        {
            var knobSize = new Vector2(NodeEditorGUI.knobSize * knobAspect, NodeEditorGUI.knobSize);
            var knobCenter = GetKnobCenter(knobSize);
            return new Rect(knobCenter.x - knobSize.x / 2, knobCenter.y - knobSize.y / 2, knobSize.x, knobSize.y);
        }

        private Vector2 GetKnobCenter(Vector2 knobSize)
        {
            var renderSide = direction == Direction.In ? inputSide : outputSide;

            var offset = 0;
            if (direction == Direction.Out)
            {
                if (renderSide == NodeSide.Left)
                    offset = 0;
                else if (renderSide == NodeSide.Right)
                    offset = -3;
                else if (renderSide == NodeSide.Top)
                    offset = 0;
                else if (renderSide == NodeSide.Bottom)
                    offset = -2;
            }

            if (renderSide == NodeSide.Left)
                return node.rect.position + new Vector2(-sideOffset - knobSize.x / 2 + offset, sidePosition);
            if (renderSide == NodeSide.Right)
                return node.rect.position + new Vector2(sideOffset + knobSize.x / 2 + node.rect.width + offset, sidePosition);
            if (renderSide == NodeSide.Bottom)
                return node.rect.position + new Vector2(sidePosition, sideOffset + knobSize.y / 2 + node.rect.height + offset);
            if (renderSide == NodeSide.Top)
                return node.rect.position + new Vector2(sidePosition, -sideOffset - knobSize.y / 2 + offset);
            throw new Exception("Unspecified nodeSide of NodeKnop " + name + ": " + inputSide);
        }

        /// <summary>
        ///     Gets the direction of the knob (vertical inverted) for connection drawing purposes
        /// </summary>
        public Vector2 GetDirection()
        {
            var renderSide = direction == Direction.In ? inputSide : outputSide;
            return renderSide == NodeSide.Right ? Vector2.right :
                renderSide == NodeSide.Bottom ? Vector2.up :
                renderSide == NodeSide.Top ? Vector2.down :
                /* Left */ Vector2.left;
        }

        /// <summary>
        ///     Gets the rotation steps anti-clockwise from NodeSide A to B
        /// </summary>
        private static int getRotationStepsAntiCW(NodeSide sideA, NodeSide sideB)
        {
            return sideB - sideA + (sideA > sideB ? 4 : 0);
        }

        #endregion

        #region Knob GUI

        /// <summary>
        ///     Draw the knob at the defined position
        /// </summary>
        public virtual void DrawKnob()
        {
            CheckKnobTexture();
            GUI.DrawTexture(GetGUIKnob(), knobTexture);
        }

        /// <summary>
        ///     Draws the connection curves from this knob to all it's connections
        /// </summary>
        public override void DrawConnections()
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var startPos = GetGUIKnob().center;
            var startDir = GetDirection();

            Debug.LogWarning("Draw knobs missing");
            // for (int i = 0; i < PortLegacy.Connections.Count; i++)
            // {
            // 	var conKnob = PortLegacy.Connections[i];
            // 	Vector2 endPos = GetGUIKnob().center;
            // 	Vector2 endDir = GetDirection();
            // 	NodeEditorGUI.DrawConnection(startPos, startDir, endPos, endDir, color);
            // }
        }

        /// <summary>
        ///     Draws a connection line from the current knob to the specified position
        /// </summary>
        public override void DrawConnection(Vector2 endPos)
        {
            var startPos = GetGUIKnob().center;
            var startDir = GetDirection();
            NodeEditorGUI.DrawConnection(startPos, startDir, endPos, -startDir, color);
        }

        /// <summary>
        ///     Displays a label and places the knob next to it, apropriately
        /// </summary>
        public void DisplayLayout()
        {
            DisplayLayout(new GUIContent(name), labelStyle);
        }

        /// <summary>
        ///     Draws a label with the knob's name and the given style. Places the knob next to it at it's nodeSide
        /// </summary>
        public void DisplayLayout(GUIStyle style)
        {
            DisplayLayout(new GUIContent(name), style);
        }

        /// <summary>
        ///     Draws a label with the given GUIContent. Places the knob next to it at it's nodeSide
        /// </summary>
        public void DisplayLayout(GUIContent content)
        {
            DisplayLayout(content, labelStyle);
        }

        /// <summary>
        ///     Draws a label with the given GUIContent and the given style. Places the knob next to it at it's nodeSide
        /// </summary>
        public void DisplayLayout(GUIContent content, GUIStyle style)
        {
            GUILayout.Label(content, style);
            SetPosition();
        }

        /// <summary>
        ///     Sets the knob's position at the specified nodeSide next to the last GUILayout control
        /// </summary>
        public void SetPosition()
        {
            if (Event.current.type != EventType.Repaint || node.ignoreGUIKnobPlacement)
                return;

            var off = 20;
            var xOff = 0; // inputSide == NodeSide.Bottom || inputSide == NodeSide.Top ? off : 0;
            var yOff = inputSide == NodeSide.Left || inputSide == NodeSide.Right ? off : 0;
            var pos = GUILayoutUtility.GetLastRect().center + new Vector2(xOff, yOff);
            //SetPosition (inputSide == NodeSide.Bottom || inputSide == NodeSide.Top? pos.x : pos.y);
            SetPosition(pos.y);
        }

        /// <summary>
        ///     Sets the knob's position at the specified nodeSide, from Top->Bottom and Left->Right
        /// </summary>
        public void SetPosition(float position, NodeSide nodeSide)
        {
            if (node.ignoreGUIKnobPlacement)
                return;
            if (inputSide != nodeSide)
            {
                inputSide = nodeSide;
                UpdateKnobTexture();
            }

            SetPosition(position);
        }

        /// <summary>
        ///     Sets the knob's position at it's nodeSide, from Top->Bottom and Left->Right
        /// </summary>
        public void SetPosition(float position)
        {
            if (node.ignoreGUIKnobPlacement)
                return;

            sidePosition = position;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConnectionKnobAttribute : ConnectionPortAttribute
    {
        public NodeSide NodeSide;
        public float NodeSidePos;

        public ConnectionKnobAttribute(string name, Direction direction) : base(name, direction)
        {
            Setup(direction == Direction.Out ? NodeSide.Right : NodeSide.Left, 0);
        }

        public ConnectionKnobAttribute(string name, Direction direction, ConnectionCount maxCount) : base(name, direction)
        {
            Setup(maxCount, direction == Direction.Out ? NodeSide.Right : NodeSide.Left, 0);
        }

        public ConnectionKnobAttribute(string name, Direction direction, string styleID) : base(name, direction, styleID)
        {
            Setup(direction == Direction.Out ? NodeSide.Right : NodeSide.Left, 0);
        }

        public ConnectionKnobAttribute(string name, Direction direction, string styleID, ConnectionCount maxCount) : base(name, direction, styleID)
        {
            Setup(maxCount, direction == Direction.Out ? NodeSide.Right : NodeSide.Left, 0);
        }

        public ConnectionKnobAttribute(string name, Direction direction, NodeSide nodeSide, float nodeSidePos = 0) : base(name, direction)
        {
            Setup(nodeSide, nodeSidePos);
        }

        public ConnectionKnobAttribute(string name, Direction direction, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0) : base(name, direction)
        {
            Setup(maxCount, nodeSide, nodeSidePos);
        }

        public ConnectionKnobAttribute(string name, Direction direction, string styleID, NodeSide nodeSide, float nodeSidePos = 0) : base(name, direction, styleID)
        {
            Setup(nodeSide, nodeSidePos);
        }

        public ConnectionKnobAttribute(string name, Direction direction, string styleID, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0) : base(name, direction, styleID)
        {
            Setup(maxCount, nodeSide, nodeSidePos);
        }

        public override Type ConnectionType => typeof(ConnectionKnob);


        private void Setup(NodeSide nodeSide, float nodeSidePos)
        {
            MaxConnectionCount = Direction == Direction.In ? ConnectionCount.Single : ConnectionCount.Multi;
            NodeSide = nodeSide;
            NodeSidePos = nodeSidePos;
        }

        private void Setup(ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos)
        {
            MaxConnectionCount = maxCount;
            NodeSide = nodeSide;
            NodeSidePos = nodeSidePos;
        }

        public override ConnectionPort CreateNew(IPort port, Node node)
        {
            var knob = ScriptableObject.CreateInstance<ConnectionKnob>();
            knob.Init(port, node, Name, Direction, NodeSide, NodeSidePos);
            knob.styleID = StyleID;
            knob.maxConnectionCount = MaxConnectionCount;
            return knob;
        }

        public override void UpdateProperties(ConnectionPort port)
        {
            var knob = (ConnectionKnob)port;
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
    public class ConnectionKnobStyle : ConnectionPortStyle
    {
        private Texture2D _inKnobTex;
        private Texture2D _outKnobTex;

        public ConnectionKnobStyle()
        {
        }

        public ConnectionKnobStyle(string name) : base(name)
        {
        }

        public virtual string InKnobTexPath => "Textures/In_Knob.png";
        public virtual string OutKnobTexPath => "Textures/Out_Knob.png";

        public Texture2D InKnobTex
        {
            get
            {
                if (_inKnobTex == null) LoadKnobTextures();
                return _inKnobTex;
            }
        }

        public Texture2D OutKnobTex
        {
            get
            {
                if (_outKnobTex == null) LoadKnobTextures();
                return _outKnobTex;
            }
        }

        protected void LoadKnobTextures()
        {
            _inKnobTex = ResourceManager.GetTintedTexture(InKnobTexPath, Color);
            _outKnobTex = ResourceManager.GetTintedTexture(OutKnobTexPath, Color);
            if (InKnobTex == null || OutKnobTex == null)
                Debug.LogError("Invalid style '" + Identifier + "': Could not load knob textures from '" + InKnobTexPath + "' and '" + OutKnobTexPath + "'!");
        }

        public override bool isValid()
        {
            return InKnobTex != null && OutKnobTex != null;
        }
    }
}