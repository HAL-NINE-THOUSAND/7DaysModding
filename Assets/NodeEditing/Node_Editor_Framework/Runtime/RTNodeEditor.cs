using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Interface;
using NodeEditing.Node_Editor_Framework.Runtime.Modals;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime
{
    public interface INodeCanvasParent
    {
        public GameObject GameObject { get; }
        public Circuit CircuitToLoad { get; set; }
    }

    /// <summary>
    ///     Example of displaying the Node Editor at runtime including GUI
    ///     Original author: https://github.com/Seneral/Node_Editor_Framework
    /// </summary>
    public class RTNodeEditor : MonoBehaviour, INodeCanvasParent
    {
        // Startup-canvas, cache and interface
        public NodeCanvas assetSave;
        public string sceneSave;

        // GUI rects
        public bool fullscreen;
        public Rect canvasRect = new(50, 50, 1800, 800);
        public NodeEditorUserCache canvasCache;
        private NodeEditorInterface editorInterface;

        public Rect rect => fullscreen ? new Rect(0, 0, Screen.width, Screen.height) : canvasRect;


        private void Start()
        {
            NormalReInit();
        }

        private void Update()
        {
            NodeEditor.Update();
        }

        private void OnGUI()
        {
            // Initiation
            NodeEditor.checkInit(true);
            if (NodeEditor.InitiationError)
            {
                GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
                return;
            }

            AssureSetup();

            // Start Overlay GUI for popups (before any other GUI)
            OverlayGUI.StartOverlayGUI("RTNodeEditor");

            // Set root rect (can be any number of arbitrary groups, e.g. a nested UI, but at least one)
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));

            // Begin Node Editor GUI and set canvas rect
            NodeEditorGUI.StartNodeGUI(false);
            canvasCache.editorState.canvasRect = new Rect(rect.x, rect.y + editorInterface.toolbarHeight, rect.width, rect.height - editorInterface.toolbarHeight);

            try
            {
                // Perform drawing with error-handling
                NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
            }
            catch (UnityException e)
            {
                // On exceptions in drawing flush the canvas to avoid locking the UI
                canvasCache.NewNodeCanvas();
                NodeEditor.ReInit(true);
                Debug.LogError("Unloaded Canvas due to exception in Draw!");
                Debug.LogException(e);
            }

            // Draw Interface
            GUILayout.BeginArea(rect);
            editorInterface.DrawToolbarGUI();
            GUILayout.EndArea();
            editorInterface.DrawModalPanel();
            ModalManager.DrawModalPanel();
            canvasCache.nodeCanvas.Messages?.DrawMessagePanel(canvasRect);

            // End Node Editor GUI
            NodeEditorGUI.EndNodeGUI();

            // End root rect
            GUI.EndGroup();

            // End Overlay GUI and draw popups
            OverlayGUI.EndOverlayGUI();
        }

        public Circuit CircuitToLoad { get; set; }

        public GameObject GameObject => gameObject;

        private void NormalReInit()
        {
            NodeEditor.ReInit(false);
            AssureSetup();
            if (canvasCache.nodeCanvas)
                canvasCache.nodeCanvas.Validate();
        }

        private void AssureSetup()
        {
            if (canvasCache == null)
                // Create cache and load startup-canvas
                canvasCache = new NodeEditorUserCache(this);
            // if (assetSave != null)
            // 	canvasCache.SetCanvas(NodeEditorSaveManager.CreateWorkingCopy(assetSave));
            // else if (!string.IsNullOrEmpty(sceneSave))
            // 	canvasCache.LoadSceneNodeCanvas(sceneSave);
            canvasCache.AssureCanvas();
            if (editorInterface == null)
            {
                // Setup editor interface
                editorInterface = new NodeEditorInterface();
                editorInterface.canvasCache = canvasCache;
            }
        }
    }
}