#if UNITY_EDITOR
#define CACHE
#endif

using System;
using System.IO;
using System.Linq;
using NodeEditing.Node_Editor_Framework.Runtime;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;

namespace NodeEditorFramework
{
    public class NodeEditorUserCache
    {
        private const string MainEditorStateIdentifier = "MainEditorState";

        public Type defaultNodeCanvasType;
        public NodeEditorState editorState;
        public NodeCanvas nodeCanvas;
        public string openedCanvasPath = "";
        public NodeCanvasTypeData typeData;

#if CACHE
        private const bool cacheWorkingCopy = true;
        private const bool cacheMemorySODump = true;
        private const int cacheIntervalSec = 60;

        private readonly bool useCache;
        private double lastCacheTime;
        private string cachePath;
        private string lastSessionPath => cachePath + "LastSession.asset";
        private string SOMemoryDumpPath => cachePath + "CurSession.asset";
#endif


        #region Setup

        public INodeCanvasParent parent;

        public NodeEditorUserCache(NodeCanvas loadedCanvas, INodeCanvasParent parentGameObject)
        {
            parent = parentGameObject;
            SetCanvas(loadedCanvas);
        }

        public NodeEditorUserCache(INodeCanvasParent parentGameObject)
        {
            parent = parentGameObject;
        }

        public NodeEditorUserCache(string CachePath, NodeCanvas loadedCanvas, INodeCanvasParent parentGameObject)
        {
#if CACHE
            useCache = true;
            cachePath = CachePath;
            SetupCacheEvents();
#endif
            parent = parentGameObject;
            SetCanvas(loadedCanvas);
        }

        public NodeEditorUserCache(string CachePath, INodeCanvasParent parentGameObject)
        {
#if CACHE
            useCache = true;
            cachePath = CachePath;
            SetupCacheEvents();
#endif
            parent = parentGameObject;
        }

        /// <summary>
        ///     Sets the cache path to a new location if cache is enabled. Does not check validity or moves files
        /// </summary>
        public void SetCachePath(string CachePath)
        {
#if CACHE
            if (useCache)
                cachePath = CachePath;
#endif
        }

        /// <summary>
        ///     Returns the cache path if cache is enabled, else an empty string
        /// </summary>
        public string GetCachePath()
        {
#if CACHE
            if (useCache)
                return cachePath;
#endif
            return "";
        }


        /// <summary>
        ///     Assures a canvas is loaded, either from the cache or new
        /// </summary>
        public void AssureCanvas()
        {
#if CACHE
            if (nodeCanvas == null)
                LoadCache();
#endif
            if (nodeCanvas == null)
                NewNodeCanvas();
            if (editorState == null)
                NewEditorState();
        }

        #endregion

        #region Cache

        /// <summary>
        ///     Subscribes the cache events needed for the cache to work properly
        /// </summary>
        public void SetupCacheEvents()
        {
#if UNITY_EDITOR && CACHE
            if (!useCache)
                return;

            EditorApplication.update -= CheckCacheUpdate;
            EditorApplication.update += CheckCacheUpdate;
            lastCacheTime = EditorApplication.timeSinceStartup;

            EditorLoadingControl.beforeEnteringPlayMode -= SaveCache;
            EditorLoadingControl.beforeEnteringPlayMode += SaveCache;
            EditorLoadingControl.beforeLeavingPlayMode -= SaveCache;
            EditorLoadingControl.beforeLeavingPlayMode += SaveCache;
            EditorLoadingControl.justEnteredPlayMode -= LoadCache;
            EditorLoadingControl.justEnteredPlayMode += LoadCache;
#endif
        }

        /// <summary>
        ///     Unsubscribes all cache events
        /// </summary>
        public void ClearCacheEvents()
        {
#if UNITY_EDITOR && CACHE
            SaveCache();
            EditorApplication.update -= CheckCacheUpdate;
            EditorLoadingControl.beforeEnteringPlayMode -= SaveCache;
            EditorLoadingControl.beforeLeavingPlayMode -= SaveCache;
            EditorLoadingControl.justEnteredPlayMode -= LoadCache;
#endif
        }

        private void CheckCacheUpdate()
        {
#if UNITY_EDITOR && CACHE
            if (EditorApplication.timeSinceStartup - lastCacheTime > cacheIntervalSec)
            {
                AssureCanvas();
                if (editorState.dragUserID == "" && editorState.connectKnob == null && GUIUtility.hotControl <= 0 && !OverlayGUI.HasPopupControl())
                {
                    // Only save when the user currently does not perform an action that could be interrupted by the save
                    lastCacheTime = EditorApplication.timeSinceStartup;
                    SaveCache();
                }
            }
#endif
        }

        /// <summary>
        ///     Creates a new cache save file for the currently loaded canvas
        ///     Only called when a new canvas is created or loaded
        /// </summary>
        private void RecreateCache()
        {
#if CACHE
            if (!useCache)
                return;
            DeleteCache();
            SaveCache();
#endif
        }

        /// <summary>
        ///     Saves the current canvas to the cache
        /// </summary>
        public void SaveCache()
        {
            SaveCache(true);
        }

        /// <summary>
        ///     Saves the current canvas to the cache
        /// </summary>
        public void SaveCache(bool crashSafe = true)
        {
#if CACHE
            if (!useCache)
                return;
            if (!nodeCanvas || nodeCanvas.GetType() == typeof(NodeCanvas))
                return;
            EditorUtility.SetDirty(nodeCanvas);
            if (editorState != null)
                EditorUtility.SetDirty(editorState);
            lastCacheTime = EditorApplication.timeSinceStartup;

            nodeCanvas.editorStates = new[] { editorState };
            if (nodeCanvas.livesInScene || nodeCanvas.allowSceneSaveOnly)
                NodeEditorSaveManager.SaveSceneNodeCanvas("lastSession", ref nodeCanvas, cacheWorkingCopy);
            else if (crashSafe)
                NodeEditorSaveManager.SaveNodeCanvas(lastSessionPath, ref nodeCanvas, cacheWorkingCopy);

            if (cacheMemorySODump)
            {
                // Functionality for asset saves only
                if (nodeCanvas.livesInScene || nodeCanvas.allowSceneSaveOnly)
                {
                    // Delete for scene save so that next cache load, correct lastSession is used
                    AssetDatabase.DeleteAsset(SOMemoryDumpPath);
                }
                else
                {
                    // Dump all SOs used in this session (even if deleted) in this file to keep them alive for undo
                    NodeEditorUndoActions.CompleteSOMemoryDump(nodeCanvas);
                    NodeEditorSaveManager.ScriptableObjectReferenceDump(nodeCanvas.SOMemoryDump, SOMemoryDumpPath, false);
                }
            }
#endif
        }

        /// <summary>
        ///     Loads the canvas from the cache save file
        ///     Called whenever a reload was made
        /// </summary>
        public void LoadCache()
        {
#if CACHE
            if (!useCache)
            {
                // Simply create a ne canvas

                NewNodeCanvas();
                return;
            }

            var skipLoad = false;
            if (cacheMemorySODump)
            {
                // Check if a memory dump has been found, if so, load that
                nodeCanvas = ResourceManager.LoadResource<NodeCanvas>(SOMemoryDumpPath);
                if (nodeCanvas != null && !nodeCanvas.Validate(false))
                {
                    Debug.LogWarning("Cache Dump corrupted! Loading crash-proof lastSession, you might have lost a bit of work. \n "
                                     + "To prevent this from happening in the future, allow the Node Editor to properly save the cache "
                                     + "by clicking out of the window before switching scenes, since there are no callbacks to facilitate this!");
                    nodeCanvas = null;
                    AssetDatabase.DeleteAsset(SOMemoryDumpPath);
                }

                if (nodeCanvas != null)
                    skipLoad = true;
            }

            // Try to load the NodeCanvas
            if (!skipLoad &&
                (!File.Exists(lastSessionPath) || (nodeCanvas = NodeEditorSaveManager.LoadNodeCanvas(lastSessionPath, cacheWorkingCopy)) == null) && // Check for asset cache
                (nodeCanvas = NodeEditorSaveManager.LoadSceneNodeCanvas("lastSession", cacheWorkingCopy)) == null) // Check for scene cache
            {
                NewNodeCanvas();
                return;
            }

            // Fetch the associated MainEditorState
            editorState = NodeEditorSaveManager.ExtractEditorState(nodeCanvas, MainEditorStateIdentifier);
            UpdateCanvasInfo();
            nodeCanvas.Validate();
            nodeCanvas.TraverseAll();
            NodeEditor.RepaintClients();
#endif
        }

#if CACHE

	    /// <summary>
	    ///     Makes sure the current canvas is saved to the cache
	    /// </summary>
	    private void CheckCurrentCache()
        {
            if (!useCache)
                return;
            if (nodeCanvas.livesInScene)
            {
                if (!NodeEditorSaveManager.HasSceneSave("lastSession"))
                    SaveCache();
            }
            else if (AssetDatabase.LoadAssetAtPath<NodeCanvas>(lastSessionPath) == null)
            {
                SaveCache();
            }
        }

	    /// <summary>
	    ///     Deletes the cache
	    /// </summary>
	    private void DeleteCache()
        {
            if (!useCache)
                return;
            AssetDatabase.DeleteAsset(SOMemoryDumpPath);
            AssetDatabase.DeleteAsset(lastSessionPath);
            AssetDatabase.Refresh();
            NodeEditorSaveManager.DeleteSceneNodeCanvas("lastSession");
        }

	    /// <summary>
	    ///     Sets the cache dirty and as makes sure it's saved
	    /// </summary>
	    private void UpdateCacheFile()
        {
            if (!useCache)
                return;
            EditorUtility.SetDirty(nodeCanvas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif

        #endregion

        #region Save/Load

        /// <summary>
        ///     Sets the current canvas, handling all cache operations
        /// </summary>
        public void SetCanvas(NodeCanvas canvas)
        {
            if (canvas == null)
            {
                NewNodeCanvas();
            }
            else if (nodeCanvas != canvas)
            {
                canvas.Validate();
                nodeCanvas = canvas;
                editorState = NodeEditorSaveManager.ExtractEditorState(nodeCanvas, MainEditorStateIdentifier);
                RecreateCache();
                UpdateCanvasInfo();
                nodeCanvas.TraverseAll();
                NodeEditor.RepaintClients();
            }
        }

        /// <summary>
        ///     Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
        /// </summary>
        public void SaveSceneNodeCanvas(string path)
        {
            nodeCanvas.editorStates = new[] { editorState };
            var switchedToScene = !nodeCanvas.livesInScene;
            NodeEditorSaveManager.SaveSceneNodeCanvas(path, ref nodeCanvas, true);
            editorState = NodeEditorSaveManager.ExtractEditorState(nodeCanvas, MainEditorStateIdentifier);
            if (switchedToScene)
                RecreateCache();
            NodeEditor.RepaintClients();
        }

        /// <summary>
        ///     Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
        /// </summary>
        public void LoadSceneNodeCanvas(string path)
        {
            if (path.StartsWith("SCENE/"))
                path = path.Substring(6);

            // Try to load the NodeCanvas
            if ((nodeCanvas = NodeEditorSaveManager.LoadSceneNodeCanvas(path, true)) == null)
            {
                NewNodeCanvas();
                return;
            }

            editorState = NodeEditorSaveManager.ExtractEditorState(nodeCanvas, MainEditorStateIdentifier);

            openedCanvasPath = path;
            nodeCanvas.Validate();
            RecreateCache();
            UpdateCanvasInfo();
            nodeCanvas.TraverseAll();
            NodeEditor.RepaintClients();
        }

        /// <summary>
        ///     Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
        /// </summary>
        public void SaveNodeCanvas(string path)
        {
            nodeCanvas.editorStates = new[] { editorState };
            var switchedToFile = nodeCanvas.livesInScene;
            NodeEditorSaveManager.SaveNodeCanvas(path, ref nodeCanvas, true);
            if (switchedToFile)
                RecreateCache();
            else
                SaveCache(false);
            NodeEditor.RepaintClients();
        }

        /// <summary>
        ///     Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
        /// </summary>
        public void LoadNodeCanvas(string path)
        {
            // Try to load the NodeCanvas
            if (!File.Exists(path) || (nodeCanvas = NodeEditorSaveManager.LoadNodeCanvas(path, true)) == null)
            {
                NewNodeCanvas();
                return;
            }

            editorState = NodeEditorSaveManager.ExtractEditorState(nodeCanvas, MainEditorStateIdentifier);

            openedCanvasPath = path;
            nodeCanvas.Validate();
            RecreateCache();
            UpdateCanvasInfo();
            nodeCanvas.TraverseAll();
            NodeEditor.RepaintClients();
        }

        /// <summary>
        ///     Creates and loads a new NodeCanvas
        /// </summary>
        public void NewNodeCanvas(Type canvasType = null)
        {
            canvasType = canvasType ?? defaultNodeCanvasType ?? // Pick first canvas in alphabetical order (Calculation usually)
                NodeCanvasManager.getCanvasDefinitions().OrderBy(c => c.DisplayString).First().CanvasType;
            nodeCanvas = NodeCanvas.CreateCanvas(canvasType);

            NewEditorState();
            openedCanvasPath = "";
            RecreateCache();
            UpdateCanvasInfo();

            var circuit = parent.CircuitToLoad ?? new Circuit();
            parent.CircuitToLoad = null;
            nodeCanvas.SetCircuit(circuit, parent);
        }

        /// <summary>
        ///     Creates a new EditorState for the current NodeCanvas
        /// </summary>
        public void NewEditorState()
        {
            editorState = ScriptableObject.CreateInstance<NodeEditorState>();
            if (!nodeCanvas) return;
            editorState.canvas = nodeCanvas;
            editorState.name = MainEditorStateIdentifier;
            nodeCanvas.editorStates = new[] { editorState };
#if UNITY_EDITOR
            EditorUtility.SetDirty(nodeCanvas);
#endif
        }

        #endregion

        #region Utility

        public void ConvertCanvasType(Type newType)
        {
            var canvas = NodeCanvasManager.ConvertCanvasType(nodeCanvas, newType);
            if (canvas != nodeCanvas)
            {
                nodeCanvas = canvas;
                RecreateCache();
                UpdateCanvasInfo();
                nodeCanvas.TraverseAll();
                NodeEditor.RepaintClients();
            }
        }

        private void UpdateCanvasInfo()
        {
            typeData = NodeCanvasManager.GetCanvasTypeData(nodeCanvas);
        }

        #endregion
    }
}