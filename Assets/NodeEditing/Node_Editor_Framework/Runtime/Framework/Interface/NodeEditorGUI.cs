using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;

namespace NodeEditorFramework
{
    public enum ConnectionDrawMethod
    {
        Bezier,
        StraightLine
    }

    public static class NodeEditorGUI
    {
        internal static bool isEditorWindow;

        // static GUI settings, textures and styles
        public static int knobSize = 16;

        public static bool useUnityEditorToolbar = false;

        public static Color NE_LightColor = new(0.4f, 0.4f, 0.4f);
        public static Color NE_TextColor = new(0.8f, 0.8f, 0.8f);
        public static Color NE_TextColorSelected = new(0.6f, 0.6f, 0.6f);

        public static Texture2D Background;
        public static Texture2D AALineTex;
        public static GUISkin overrideSkin;
        private static GUISkin defaultSkin;
        private static GUISkin unitySkin;

        public static Dictionary<string, GUIStyle> GUIStyles = new();
        private static Color unityTextColor, unityHoverTextColor, unityActiveTextColor, unityFocusedTextColor;

        public static GUISkin nodeSkin => overrideSkin ?? defaultSkin;


        public static bool Init()
        {
            overrideSkin = ResourceManager.LoadResource<GUISkin>("OverrideSkin.asset");
            if (overrideSkin == null)
            {
                // Create default skin from scratch
                if (!CreateDefaultSkin()) return false;
                overrideSkin = Object.Instantiate(defaultSkin);
            }
            else
            {
                // Use override
                overrideSkin = Object.Instantiate(overrideSkin);
            }

            // Copy default styles in current setting, modified to fit custom style
            // This mostly refers to the editor styles

            var customStyles = new List<GUIStyle>(overrideSkin.customStyles);
            foreach (var style in GUI.skin.customStyles)
                if (overrideSkin.FindStyle(style.name) == null)
                {
                    var modStyle = new GUIStyle(style);
                    if (modStyle.normal.background == null)
                    {
                        modStyle.fontSize = overrideSkin.label.fontSize;
                        modStyle.normal.textColor = modStyle.active.textColor = modStyle.focused.textColor = modStyle.hover.textColor = overrideSkin.label.normal.textColor;
                    }

                    customStyles.Add(modStyle);
                }

            overrideSkin.customStyles = customStyles.ToArray();

            Background = ResourceManager.LoadTexture("Textures/background.png");
            AALineTex = ResourceManager.LoadTexture("Textures/AALine.png");

            return Background && AALineTex;
        }

        public static bool CreateDefaultSkin()
        {
            // Textures
            Background = ResourceManager.LoadTexture("Textures/background.png");
            AALineTex = ResourceManager.LoadTexture("Textures/AALine.png");
            var GUIBox = ResourceManager.LoadTexture("Textures/NE_Box.png");
            var GUISelectedBG = ResourceManager.LoadTexture("Textures/NE_SelectedBG.png");
            var GUIButton = ResourceManager.LoadTexture("Textures/NE_Button.png");
            var GUIButtonHover = ResourceManager.LoadTexture("Textures/NE_Button_Hover.png");
            var GUIButtonSelected = ResourceManager.LoadTexture("Textures/NE_Button_Selected.png");
            var GUIToolbar = ResourceManager.LoadTexture("Textures/NE_Toolbar.png");
            var GUIToolbarButton = ResourceManager.LoadTexture("Textures/NE_ToolbarButton.png");
            var GUIToolbarLabel = ResourceManager.LoadTexture("Textures/NE_ToolbarLabel.png");

            if (!Background || !AALineTex || !GUIBox || !GUIButton || !GUIToolbar || !GUIToolbarButton)
                return false;

            // Skin & Styles
            GUI.skin = null;
            defaultSkin = Object.Instantiate(GUI.skin);

            foreach (GUIStyle style in defaultSkin) style.fontSize = 12;
            //style.normal.textColor = style.active.textColor = style.focused.textColor = style.hover.textColor = NE_TextColor;
            var customStyles = new List<GUIStyle>();

            // Label
            defaultSkin.label.normal.textColor = NE_TextColor;
            var boxStyle = new GUIStyle(nodeSkin.box);
            boxStyle.contentOffset = new Vector2(2, 2);
            customStyles.Add(boxStyle);
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "labelBold", fontStyle = FontStyle.Bold });
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "labelCentered", alignment = TextAnchor.MiddleCenter });
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "labelBoldCentered", alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "labelLeft", alignment = TextAnchor.MiddleLeft });
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "labelRight", alignment = TextAnchor.MiddleRight });
            customStyles.Add(new GUIStyle(defaultSkin.label) { name = "actionButton", alignment = TextAnchor.MiddleCenter, fontSize = 22 });
            var labelSelected = new GUIStyle(defaultSkin.label) { name = "labelSelected" };
            labelSelected.normal.background = GUISelectedBG;
            customStyles.Add(labelSelected);

            // Box
            defaultSkin.box.normal.background = GUIBox;
            defaultSkin.box.normal.textColor = NE_TextColor;
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
            defaultSkin.box.normal.scaledBackgrounds = null;
#endif
            defaultSkin.box.active.textColor = NE_TextColor;
            customStyles.Add(new GUIStyle(defaultSkin.box) { name = "boxBold", fontStyle = FontStyle.Bold });

            // Button
            defaultSkin.button.normal.textColor = defaultSkin.button.active.textColor = defaultSkin.button.focused.textColor = defaultSkin.button.hover.textColor = NE_TextColor;
            defaultSkin.button.normal.background = GUIButton;
            defaultSkin.button.hover.background = GUIButtonHover;
            defaultSkin.button.active.background = GUIButtonSelected;
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
            defaultSkin.button.normal.scaledBackgrounds = null;
            defaultSkin.button.hover.scaledBackgrounds = null;
            defaultSkin.button.active.scaledBackgrounds = null;
#endif
            defaultSkin.button.border = new RectOffset(1, 1, 1, 1);
            defaultSkin.button.margin = new RectOffset(2, 2, 1, 1);
            defaultSkin.button.padding = new RectOffset(4, 4, 1, 1);
            defaultSkin.button.fontSize = 12;

            // Toolbar
            if (useUnityEditorToolbar && defaultSkin.FindStyle("toolbar") != null && defaultSkin.FindStyle("toolbarButton") != null && defaultSkin.FindStyle("toolbarDropdown") != null)
            {
                customStyles.Add(new GUIStyle(defaultSkin.GetStyle("toolbar")));
                customStyles.Add(new GUIStyle(defaultSkin.GetStyle("toolbarButton")));
                customStyles.Add(new GUIStyle(defaultSkin.GetStyle("toolbarDropdown")));
                customStyles.Add(new GUIStyle(defaultSkin.GetStyle("toolbarButton")) { name = "toolbarLabel" });
            }
            else
            {
                // No editor style - use custom skin
                var toolbar = new GUIStyle(defaultSkin.box) { name = "toolbar" };
                toolbar.normal.background = GUIToolbar;
                toolbar.active.background = GUIToolbar;
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
                toolbar.normal.scaledBackgrounds = null;
                toolbar.active.scaledBackgrounds = null;
#endif
                toolbar.border = new RectOffset(0, 0, 1, 1);
                toolbar.margin = new RectOffset(0, 0, 0, 0);
                toolbar.padding = new RectOffset(1, 1, 1, 1);
                toolbar.overflow = new RectOffset(0, 0, 0, 1);
                customStyles.Add(toolbar);

                var toolbarLabel = new GUIStyle(defaultSkin.box) { name = "toolbarLabel" };
                toolbarLabel.normal.background = GUIToolbarLabel;
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
                toolbarLabel.normal.scaledBackgrounds = null;
#endif
                toolbarLabel.border = new RectOffset(2, 2, 0, 0);
                toolbarLabel.margin = new RectOffset(0, 0, 0, 0);
                toolbarLabel.padding = new RectOffset(6, 6, 2, 2);
                customStyles.Add(toolbarLabel);

                var toolbarButton = new GUIStyle(toolbarLabel) { name = "toolbarButton" };
                toolbarButton.normal.background = GUIToolbarButton;
                toolbarButton.active.background = GUISelectedBG;
                customStyles.Add(toolbarButton);

                var toolbarDropdown = new GUIStyle(toolbarButton) { name = "toolbarDropdown" };
                customStyles.Add(toolbarDropdown);
            }

            // Delete Editor-only resources
            defaultSkin.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            defaultSkin.window.normal.background = null;
            defaultSkin.window.onNormal.background = null;


            defaultSkin.customStyles = customStyles.ToArray();


            foreach (var style in customStyles)
                if (!GUIStyles.ContainsKey(style.name))
                    GUIStyles.Add(style.name, style);

#if UNITY_EDITOR
            if (!ResourceManager.resourcePath.StartsWith("Packages"))
                AssetDatabase.CreateAsset(Object.Instantiate(defaultSkin), ResourceManager.resourcePath + "DefaultSkin.asset");
#endif

            return true;
        }

        public static void StartNodeGUI(bool IsEditorWindow)
        {
            NodeEditor.checkInit(true);
            // Required for gamemode switch
            // Also for EditorWindow+RTNodeEditor in parallel where RTNodeEditor GUISkin setup would not be enough for the editor window as it's missing the editor styles
            if (nodeSkin == null || (IsEditorWindow && nodeSkin.FindStyle("ObjectField") == null))
                NodeEditor.ReInit(true);

            isEditorWindow = IsEditorWindow;

            unitySkin = GUI.skin;
            GUI.skin = nodeSkin;
#if UNITY_EDITOR
            unityTextColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = NE_TextColor;
            unityHoverTextColor = EditorStyles.label.hover.textColor;
            EditorStyles.label.hover.textColor = NE_TextColor;
            unityActiveTextColor = EditorStyles.label.active.textColor;
            EditorStyles.label.active.textColor = NE_TextColorSelected;
            unityFocusedTextColor = EditorStyles.label.focused.textColor;
            EditorStyles.label.focused.textColor = NE_TextColorSelected;
#endif
        }

        public static void EndNodeGUI()
        {
            GUI.skin = unitySkin;
#if UNITY_EDITOR
            EditorStyles.label.normal.textColor = unityTextColor;
            EditorStyles.label.hover.textColor = unityHoverTextColor;
            EditorStyles.label.active.textColor = unityActiveTextColor;
            EditorStyles.label.focused.textColor = unityFocusedTextColor;
#endif
        }

        /// <summary>
        ///     Unified method to generate a random HSV color value across versions
        /// </summary>
        public static Color RandomColorHSV(int seed, float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
        {
            // Set seed
#if UNITY_5_4_OR_NEWER
            Random.InitState(seed);
#else
			UnityEngine.Random.seed = seed;
#endif
            // Consistent random H,S,V values
            var hue = Random.Range(hueMin, hueMax);
            var saturation = Random.Range(saturationMin, saturationMax);
            var value = Random.Range(valueMin, valueMax);

            // Convert HSV to RGB
#if UNITY_5_3_OR_NEWER
            return Color.HSVToRGB(hue, saturation, value, false);
#else
			int hi = Mathf.FloorToInt(hue / 60) % 6;
			float frac = hue / 60 - Mathf.Floor(hue / 60);

			float v = value;
			float p = value * (1 - saturation);
			float q = value * (1 - frac * saturation);
			float t = value * (1 - (1 - frac) * saturation);

			if (hi == 0)
				return new Color(v, t, p);
			else if (hi == 1)
				return new Color(q, v, p);
			else if (hi == 2)
				return new Color(p, v, t);
			else if (hi == 3)
				return new Color(p, q, v);
			else if (hi == 4)
				return new Color(t, p, v);
			else
				return new Color(v, p, q);
#endif
        }

        #region Connection Drawing

        // Curve parameters
        public static float curveBaseDirection = 1.5f, curveBaseStart = 2f, curveDirectionScale = 0.004f;

        /// <summary>
        ///     Draws a node connection from start to end, horizontally
        /// </summary>
        public static void DrawConnection(Vector2 startPos, Vector2 endPos, Color col)
        {
            var startVector = startPos.x <= endPos.x ? Vector2.right : Vector2.left;
            DrawConnection(startPos, startVector, endPos, -startVector, col);
        }

        /// <summary>
        ///     Draws a node connection from start to end, horizontally
        /// </summary>
        public static void DrawConnection(Vector2 startPos, Vector2 endPos, ConnectionDrawMethod drawMethod, Color col)
        {
            var startVector = startPos.x <= endPos.x ? Vector2.right : Vector2.left;
            DrawConnection(startPos, startVector, endPos, -startVector, drawMethod, col);
        }

        /// <summary>
        ///     Draws a node connection from start to end with specified vectors
        /// </summary>
        public static void DrawConnection(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, Color col)
        {
#if NODE_EDITOR_LINE_CONNECTION
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.StraightLine, col);
#else
            DrawConnection(startPos, startDir, endPos, endDir, ConnectionDrawMethod.Bezier, col);
#endif
        }

        /// <summary>
        ///     Draws a node connection from start to end with specified vectors
        /// </summary>
        public static void DrawConnection(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, ConnectionDrawMethod drawMethod, Color col)
        {
            if (drawMethod == ConnectionDrawMethod.Bezier)
            {
                OptimiseBezierDirections(startPos, ref startDir, endPos, ref endDir);
                float dirFactor = 80; //Mathf.Pow ((startPos-endPos).magnitude, 0.3f) * 20;
                //Debug.Log ("DirFactor is " + dirFactor + "with a bezier lenght of " + (startPos-endPos).magnitude);
                RTEditorGUI.DrawBezier(startPos, endPos, startPos + startDir * dirFactor, endPos + endDir * dirFactor, col * Color.gray, null, 3);
            }
            else if (drawMethod == ConnectionDrawMethod.StraightLine)
            {
                RTEditorGUI.DrawLine(startPos, endPos, col * Color.gray, null, 3);
            }
        }

        /// <summary>
        ///     Optimises the bezier directions scale so that the bezier looks good in the specified position relation.
        ///     Only the magnitude of the directions are changed, not their direction!
        /// </summary>
        public static void OptimiseBezierDirections(Vector2 startPos, ref Vector2 startDir, Vector2 endPos, ref Vector2 endDir)
        {
            var offset = (endPos - startPos) * curveDirectionScale;
            var baseDir = Mathf.Min(offset.magnitude / curveBaseStart, 1) * curveBaseDirection;
            var scale = new Vector2(Mathf.Abs(offset.x) + baseDir, Mathf.Abs(offset.y) + baseDir);
            // offset.x and offset.y linearly increase at scale of curveDirectionScale
            // For 0 < offset.magnitude < curveBaseStart, baseDir linearly increases from 0 to curveBaseDirection. For offset.magnitude > curveBaseStart, baseDir = curveBaseDirection
            startDir = Vector2.Scale(startDir.normalized, scale);
            endDir = Vector2.Scale(endDir.normalized, scale);
        }

        /// <summary>
        ///     Gets the second connection vector that matches best, accounting for positions
        /// </summary>
        internal static Vector2 GetSecondConnectionVector(Vector2 startPos, Vector2 endPos, Vector2 firstVector)
        {
            if (firstVector.x != 0 && firstVector.y == 0)
                return startPos.x <= endPos.x ? -firstVector : firstVector;
            if (firstVector.y != 0 && firstVector.x == 0)
                return startPos.y <= endPos.y ? -firstVector : firstVector;
            return -firstVector;
        }

        #endregion
    }
}