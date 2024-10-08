﻿using NodeEditing.Node_Editor_Framework.Runtime.Framework.Interface;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Examples.FrameworkExtends
{
	/// <summary>
	/// Example of displaying the Node Editor at runtime including GUI.
	/// Modified to reference a custom state variable in extended NodeEditorState using partial keyword.
	/// </summary>
	public class ExtendedRTNodeEditor : MonoBehaviour 
	{
		// Startup-canvas, cache and interface
		public NodeCanvas assetSave;
		public string sceneSave;
		private NodeEditorUserCache canvasCache;
		private NodeEditorInterface editorInterface;

		// GUI rects
		public bool fullscreen = false;
		public Rect canvasRect = new Rect(50, 50, 1800, 800);
		public Rect rect { get { return fullscreen ? new Rect(0, 0, Screen.width, Screen.height) : canvasRect; } }


		private void Start () 
		{
			NormalReInit();
		}

		private void Update () 
		{
			NodeEditor.Update ();
		}

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
			{ // Create cache and load startup-canvas
				canvasCache = new NodeEditorUserCache(null);
				if (assetSave != null)
					canvasCache.SetCanvas(NodeEditorSaveManager.CreateWorkingCopy(assetSave));
				else if (!string.IsNullOrEmpty (sceneSave))
					canvasCache.LoadSceneNodeCanvas(sceneSave);
			}
			canvasCache.AssureCanvas();
			if (editorInterface == null)
			{ // Setup editor interface
				editorInterface = new NodeEditorInterface();
				editorInterface.canvasCache = canvasCache;
			}
		}

		private void OnGUI ()
		{
			// Initiation
			NodeEditor.checkInit(true);
			if (NodeEditor.InitiationError)
			{
				GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
				return;
			}
			AssureSetup();

			// Start Overlay GUI for popups
			OverlayGUI.StartOverlayGUI("RTNodeEditor");

			// Begin Node Editor GUI and set canvas rect
			NodeEditorGUI.StartNodeGUI(false);
			canvasCache.editorState.canvasRect = new Rect (rect.x, rect.y + editorInterface.toolbarHeight, rect.width, rect.height - editorInterface.toolbarHeight);

			// Access custom state variable whenever you need
			canvasCache.editorState.myCustomStateVariable = 0;

			try
			{ // Perform drawing with error-handling
				NodeEditor.DrawCanvas (canvasCache.nodeCanvas, canvasCache.editorState);
			}
			catch (UnityException e)
			{ // On exceptions in drawing flush the canvas to avoid locking the UI
				canvasCache.NewNodeCanvas ();
				NodeEditor.ReInit (true);
				Debug.LogError ("Unloaded Canvas due to exception in Draw!");
				Debug.LogException (e);
			}
		
			// Draw Interface
			GUILayout.BeginArea(rect);
			editorInterface.DrawToolbarGUI();
			GUILayout.EndArea();
			editorInterface.DrawModalPanel();

			// End Node Editor GUI
			NodeEditorGUI.EndNodeGUI();
		
			// End Overlay GUI and draw popups
			OverlayGUI.EndOverlayGUI();
		}
	}
}