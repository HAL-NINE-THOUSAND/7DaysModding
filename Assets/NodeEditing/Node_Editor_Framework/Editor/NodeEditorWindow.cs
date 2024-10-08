﻿using UnityEngine;
using UnityEditor;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Interface;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
	public class NodeEditorWindow : EditorWindow 
	{

		/* Instructions for custom windows! Read if you copy this code to create a custom window for your tool

		If you use NodeEditorUserCache to cache and manage the session files, you will need a path to save the curSession and lastSession files to
		You can continue using the default temp folder, but then your window will share the session with the default window, which might not be desired
		To change the temp folder of this window, there are several options, depending on whether you intent to distribute through the Unity Package Manager or not:
		
		Distribution through Unity Package Manager:
		The temp folder HAS to be in the Assets folder, so the Packages/ subfolder won't do. Here are your options:
		1. Put your temp folder in a subfolder of the existing temp folder by writing to TEMP_PATH_SUBFOLDER (e.g. "Texture_Composer/") (RECOMMENDED)
		2. Change TEMP_PATH_MARKER_GUID to any GUID generated by Unity and change the default location in TEMP_PATH_DEFAULT
			To generate a new GUID, create any file, read GUID from .meta file, and delete the file
			This will allow users to move your temp folder individually by moving a specifically created marker file
		// Both base upon a new variable temp path system, only required if we have no folder in Assets/ that we can call ours to store the sessions in
		// It will start with a default folder, but will allow users to easily move it by moving the files and a marker to any folder in Assets/

		Normal Distribution as unitypackage or through the Asset Store:
		You can use your editorPath where the scripts are stored as a base for storing the temp files. Two options:
		1. Do not change anything and use NodeEditor.editorPath as storage folder (DEFAULT, RECOMMENDED)
			Optionally store in a subfolder specified by TEMP_PATH_SUBFOLDER (e.g. "Texture_Composer/")
			ANY editor tool that embeds the Node Editor Framework in a custom location needs to change NodeEditor.editorPath anyway
		3. Change TEMP_PATH_FIXED to a fixed path in the Assets folder, preferrably in your own tools folder
		*/
		private const bool TEMP_PATH_USE_EDITOR_PATH_IF_IN_ASSETS = true; // If NodeEditor.editorPath starts with Assets/, use that as a base folder
		private const string TEMP_PATH_MARKER_GUID = "7b443eac9ba200a4d8d0c7640900a150"; // Marker GUID of default Node Editor Window
		private const string TEMP_PATH_DEFAULT = "Assets/DefaultEditorResources/Node_Editor_Framework/";
		private const string TEMP_PATH_SUBFOLDER = null; // e.g. Texture_Composer/
		private const string TEMP_PATH_FIXED = null;
		private const string META_FILE = 
@"fileFormatVersion: 2
guid: MARKER_GUID
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
 	
		// Information about current instance
		private static NodeEditorWindow _editor;
		public static NodeEditorWindow editor { get { AssureEditor(); return _editor; } }
		public static void AssureEditor() { if (_editor == null) OpenNodeEditor(); }

		// Canvas cache
		public NodeEditorUserCache canvasCache;
		public NodeEditorInterface editorInterface;

		// GUI
		private Rect canvasWindowRect { get { return new Rect(0, editorInterface.toolbarHeight, position.width, position.height - editorInterface.toolbarHeight); } }


		#region General 

		/// <summary>
		/// Opens the Node Editor window and loads the last session
		/// </summary>
		[MenuItem("Window/Node Editor")]
		public static NodeEditorWindow OpenNodeEditor () 
		{
			_editor = GetWindow<NodeEditorWindow>();
			_editor.minSize = new Vector2(400, 200);

			NodeEditor.ReInit (false);
			Texture iconTexture = ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent ("Node Editor", iconTexture);

			return _editor;
		}

		/*
		/// <summary>
		/// Assures that the canvas is opened when double-clicking a canvas asset
		/// </summary>
		[UnityEditor.Callbacks.OnOpenAsset(1)]
		private static bool AutoOpenCanvas(int instanceID, int line)
		{
			if (Selection.activeObject != null && Selection.activeObject is NodeCanvas)
			{
				string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
				OpenNodeEditor().canvasCache.LoadNodeCanvas(NodeCanvasPath);
				return true;
			}
			return false;
		}
		*/
			
		private void OnEnable()
		{
			_editor = this;
			NormalReInit();

			// Subscribe to events
			NodeEditor.ClientRepaints -= Repaint;
			NodeEditor.ClientRepaints += Repaint;
			EditorLoadingControl.justLeftPlayMode -= NormalReInit;
			EditorLoadingControl.justLeftPlayMode += NormalReInit;
			EditorLoadingControl.justOpenedNewScene -= NormalReInit;
			EditorLoadingControl.justOpenedNewScene += NormalReInit;
		#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;
		#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		#endif
			Undo.undoRedoPerformed -= NodeEditor.RepaintClients;
			Undo.undoRedoPerformed += NodeEditor.RepaintClients;
			Undo.undoRedoPerformed -= UndoRedoRecalculate;
			Undo.undoRedoPerformed += UndoRedoRecalculate;
		}
		
		private void OnDestroy()
		{
			// Unsubscribe from events
			NodeEditor.ClientRepaints -= Repaint;
			EditorLoadingControl.justLeftPlayMode -= NormalReInit;
			EditorLoadingControl.justOpenedNewScene -= NormalReInit;
		#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
		#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		#endif
			Undo.undoRedoPerformed -= NodeEditor.RepaintClients;
			Undo.undoRedoPerformed -= UndoRedoRecalculate;

			// Clear Cache
			canvasCache.ClearCacheEvents();
		}

		private void UndoRedoRecalculate()
		{
			canvasCache.nodeCanvas.TraverseAll();
		}

		private void OnLostFocus () 
		{ // Save any changes made while focussing this window
			// Will also save before possible assembly reload, scene switch, etc. because these require focussing of a different window
			canvasCache.SaveCache(false);
		}

		private void OnFocus () 
		{ // Make sure the canvas hasn't been corrupted externally
			NormalReInit();
		}

		private void NormalReInit()
		{
			NodeEditor.ReInit(false);
			AssureCache();
			AssureSetup();
			if (canvasCache.nodeCanvas)
			{
				if (!canvasCache.nodeCanvas.Validate(false))
					canvasCache.LoadCache();
			}
		}

		private void AssureSetup()
		{
			if (canvasCache == null)
				AssureCache();
			else if (!AssetDatabase.IsValidFolder(ResourceManager.StripTrailingSeparator(canvasCache.GetCachePath())))
				AssureCache();
			canvasCache.AssureCanvas();
			
			if (editorInterface == null)
			{ // Setup editor interface
				editorInterface = new NodeEditorInterface();
				editorInterface.canvasCache = canvasCache;
				editorInterface.ShowNotificationAction = ShowNotification;
			}
		}
		private void AssureCache()
		{
			// Get temp path to save cache to
			string tempPath;
			if (TEMP_PATH_USE_EDITOR_PATH_IF_IN_ASSETS && NodeEditor.editorPath.StartsWith("Assets") 
			&& AssetDatabase.IsValidFolder(ResourceManager.StripTrailingSeparator(NodeEditor.editorPath)))
			{
				tempPath = NodeEditor.editorPath;
			}
			else if (!string.IsNullOrEmpty(TEMP_PATH_FIXED)) {
				tempPath = TEMP_PATH_FIXED;
				Directory.CreateDirectory(tempPath);
			}
			else
			{ // Use variable temp path, only required if we have no folder in Assets/ that we can call ours to store the sessions in
			// It will start with a default folder, but will allow users to easily move it by moving the files and a marker to any folder in Assets/
				// 1. Try to find temp path marker
				tempPath = AssetDatabase.GUIDToAssetPath(TEMP_PATH_MARKER_GUID);
				// Sometimes this will return a path but the asset behind it has been deleted
				if (!string.IsNullOrEmpty(tempPath) && !AssetDatabase.IsValidFolder(ResourceManager.UnifyPathSeparators(Path.GetDirectoryName(tempPath), '/')))
					tempPath = "";
				if (string.IsNullOrEmpty(tempPath) || !File.Exists(tempPath))
				{ // 2. Create temp path marker with specified GUID
					if (string.IsNullOrEmpty(tempPath)) 
					{ // No previous folder trace to use
						Directory.CreateDirectory(TEMP_PATH_DEFAULT);
						tempPath = TEMP_PATH_DEFAULT;
					}
					using (File.Create(tempPath + "NEFTempFilesMarker")) {}
					using (StreamWriter sw = File.CreateText(tempPath + "NEFTempFilesMarker.meta")) 
						sw.Write(META_FILE.Replace("MARKER_GUID", TEMP_PATH_MARKER_GUID));
					AssetDatabase.Refresh();
					tempPath = AssetDatabase.GUIDToAssetPath(TEMP_PATH_MARKER_GUID);
					Debug.LogWarning("Created temp marker '" + tempPath + "'! You can move this marker along with the cache files curSession and lastSession to a different cache location.");
				}
				tempPath = ResourceManager.UnifyPathSeparators(Path.GetDirectoryName(tempPath), '/') + "/";
			}
			if (!string.IsNullOrEmpty(TEMP_PATH_SUBFOLDER)) 
			{ // 3. Apply subfolder
				tempPath = tempPath + TEMP_PATH_SUBFOLDER;
				Directory.CreateDirectory(tempPath);
			}

			// Make sure we have a cache at that temp path with a canvas
			if (canvasCache == null)
				canvasCache = new NodeEditorUserCache(tempPath, null);
			else
				canvasCache.SetCachePath(tempPath);
			canvasCache.AssureCanvas();
		}

		#endregion

		#region GUI

		private void OnGUI()
		{
			// Initiation
			NodeEditor.checkInit(true);
			if (NodeEditor.InitiationError)
			{
				GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
				return;
			}
			AssureEditor ();
			AssureSetup();

			// ROOT: Start Overlay GUI for popups
			OverlayGUI.StartOverlayGUI("NodeEditorWindow");

			// Begin Node Editor GUI and set canvas rect
			NodeEditorGUI.StartNodeGUI(true);
			canvasCache.editorState.canvasRect = canvasWindowRect;

			try
			{ // Perform drawing with error-handling
				NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
			}
			catch (UnityException e)
			{ // On exceptions in drawing flush the canvas to avoid locking the UI
				canvasCache.NewNodeCanvas();
				NodeEditor.ReInit(true);
				Debug.LogError("Unloaded Canvas due to an exception during the drawing phase!");
				Debug.LogException(e);
			}

			// Draw Interface
			editorInterface.DrawToolbarGUI();
			editorInterface.DrawModalPanel();

			// End Node Editor GUI
			NodeEditorGUI.EndNodeGUI();
			
			// END ROOT: End Overlay GUI and draw popups
			OverlayGUI.EndOverlayGUI();
		}

		private void OnSceneGUI(SceneView sceneview)
		{
			AssureSetup();
			if (canvasCache.editorState != null && canvasCache.editorState.selectedNode != null)
				canvasCache.editorState.selectedNode.OnSceneGUI();
			SceneView.lastActiveSceneView.Repaint();
		}

		#endregion
	}
}