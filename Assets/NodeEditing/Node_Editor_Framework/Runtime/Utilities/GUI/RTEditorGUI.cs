﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeEditorFramework.Utilities
{
    public static class RTEditorGUI
    {
        #region GUI Proportioning Utilities

        public static float labelWidth = 150;
        public static float fieldWidth = 50;
        public static float indent = 0;
        private static float textFieldHeight => GUI.skin.textField.CalcHeight(new GUIContent("i"), 10);

        public static Rect PrefixLabel(Rect totalPos, GUIContent label, GUIStyle style)
        {
            if (label == GUIContent.none)
                return totalPos; //IndentedRect (totalPos);

            var labelPos = new Rect(totalPos.x + indent, totalPos.y, Mathf.Min(getLabelWidth() - indent, totalPos.width / 2), totalPos.height);
            GUI.Label(labelPos, label, style);

            return new Rect(totalPos.x + getLabelWidth(), totalPos.y, totalPos.width - getLabelWidth(), totalPos.height);
        }

        public static Rect PrefixLabel(Rect totalPos, float percentage, GUIContent label, GUIStyle style)
        {
            if (label == GUIContent.none)
                return totalPos;

            var labelPos = new Rect(totalPos.x + indent, totalPos.y, totalPos.width * percentage, totalPos.height);
            GUI.Label(labelPos, label, style);

            return new Rect(totalPos.x + totalPos.width * percentage, totalPos.y, totalPos.width * (1 - percentage), totalPos.height);
        }

        private static Rect IndentedRect(Rect source)
        {
            return new Rect(source.x + indent, source.y, source.width - indent, source.height);
        }

        private static float getLabelWidth()
        {
#if UNITY_EDITOR
            return EditorGUIUtility.labelWidth;
#else
			if (labelWidth == 0)
			return 150;
			return labelWidth;
#endif
        }

        private static float getFieldWidth()
        {
#if UNITY_EDITOR
            return EditorGUIUtility.fieldWidth;
#else
			if (fieldWidth == 0)
			return 50;
			return fieldWidth;
#endif
        }

        private static Rect GetFieldRect(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
        {
            float minLabelW = 0, maxLabelW = 0;
            if (label != GUIContent.none)
                style.CalcMinMaxWidth(label, out minLabelW, out maxLabelW);
            return GUILayoutUtility.GetRect(getFieldWidth() + minLabelW + 5, getFieldWidth() + maxLabelW + 5, textFieldHeight, textFieldHeight, options);
        }

        private static Rect GetSliderRect(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
        {
            float minLabelW = 0, maxLabelW = 0;
            if (label != GUIContent.none)
                style.CalcMinMaxWidth(label, out minLabelW, out maxLabelW);
            return GUILayoutUtility.GetRect(getFieldWidth() + minLabelW + 5, getFieldWidth() + maxLabelW + 5 + 100, textFieldHeight, textFieldHeight, options);
        }

        private static Rect GetSliderRect(Rect sliderRect)
        {
            return new Rect(sliderRect.x, sliderRect.y, sliderRect.width - getFieldWidth() - 5, sliderRect.height);
        }

        private static Rect GetSliderFieldRect(Rect sliderRect)
        {
            return new Rect(sliderRect.x + sliderRect.width - getFieldWidth(), sliderRect.y, getFieldWidth(), sliderRect.height);
        }

        #endregion

        #region Seperator

        /// <summary>
        ///     Efficient space like EditorGUILayout.Space
        /// </summary>
        public static void Space()
        {
            Space(6);
        }

        /// <summary>
        ///     Space like GUILayout.Space but more efficient
        /// </summary>
        public static void Space(float pixels)
        {
            GUILayoutUtility.GetRect(pixels, pixels);
        }


        /// <summary>
        ///     A GUI Function which simulates the default seperator
        /// </summary>
        public static void Seperator()
        {
            setupSeperator();
            GUILayout.Box(GUIContent.none, seperator, GUILayout.Height(1));
        }

        /// <summary>
        ///     A GUI Function which simulates the default seperator
        /// </summary>
        public static void Seperator(Rect rect)
        {
            setupSeperator();
            GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), GUIContent.none, seperator);
        }

        private static GUIStyle seperator;

        private static void setupSeperator()
        {
            if (seperator == null)
            {
                seperator = new GUIStyle();
                seperator.normal.background = ColorToTex(1, new Color(0.6f, 0.6f, 0.6f));
                seperator.stretchWidth = true;
                seperator.margin = new RectOffset(0, 0, 7, 7);
            }
        }

        #endregion

        #region Change Check

        private static readonly Stack<bool> changeStack = new();

        public static void BeginChangeCheck()
        {
            changeStack.Push(GUI.changed);
            GUI.changed = false;
        }

        public static bool EndChangeCheck()
        {
            var changed = GUI.changed;
            if (changeStack.Count > 0)
            {
                GUI.changed = changeStack.Pop();
                if (changed && changeStack.Count > 0 && !changeStack.Peek())
                {
                    // Update parent change check
                    changeStack.Pop();
                    changeStack.Push(changed);
                }
            }
            else
            {
                Debug.LogWarning("Requesting more EndChangeChecks than issuing BeginChangeChecks!");
            }

            return changed;
        }

        #endregion


        #region Foldout and Toggle Wrappers

        public static bool Foldout(bool foldout, string content, params GUILayoutOption[] options)
        {
            return Foldout(foldout, new GUIContent(content), options);
        }

        public static bool Foldout(bool foldout, string content, GUIStyle style, params GUILayoutOption[] options)
        {
            return Foldout(foldout, new GUIContent(content), style, options);
        }

        public static bool Foldout(bool foldout, GUIContent content, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.Foldout(foldout, content);
#endif
            return Foldout(foldout, content, GUI.skin.toggle, options);
        }

        public static bool Foldout(bool foldout, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.Foldout(foldout, content, style);
#endif
            return GUILayout.Toggle(foldout, content, style, options);
        }


        public static bool Toggle(bool toggle, string content, params GUILayoutOption[] options)
        {
            return Toggle(toggle, new GUIContent(content), options);
        }

        public static bool Toggle(bool toggle, string content, GUIStyle style, params GUILayoutOption[] options)
        {
            return Toggle(toggle, new GUIContent(content), style, options);
        }

        public static bool Toggle(bool toggle, GUIContent content, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.ToggleLeft(content, toggle, options);
#endif
            return Toggle(toggle, content, GUI.skin.toggle, options);
        }

        public static bool Toggle(bool toggle, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.ToggleLeft(content, toggle, style, options);
#endif
            return GUILayout.Toggle(toggle, content, style, options);
        }

        #endregion

        #region Fields and Sliders

        #region Extra

        /// <summary>
        ///     Text Field with label for ingame purposes with copy-paste functionality. Should behave like
        ///     UnityEditor.EditorGUILayout.TextField
        /// </summary>
        public static string TextField(string text, params GUILayoutOption[] options)
        {
            return TextField(GUIContent.none, text, null, options);
        }

        /// <summary>
        ///     Text Field with label for ingame purposes with copy-paste functionality. Should behave like
        ///     UnityEditor.EditorGUILayout.TextField
        /// </summary>
        public static string TextField(GUIContent label, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            /*#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorFramework.NodeEditorGUI.isEditorWindow)
                return UnityEditor.EditorGUILayout.TextField (label, text, options);
            #endif*/

            if (style == null) style = GUI.skin.textField;
            if (text == null) text = "";

            var totalPos = GetFieldRect(label, style, options);
            var fieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);

            // Handle custom copy-paste
            text = HandleCopyPaste(GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard, fieldPos) + 1) ?? text;
            text = GUI.TextField(fieldPos, text);
            return text;
        }


        /// <summary>
        ///     Slider to select between the given options
        /// </summary>
        public static int OptionSlider(GUIContent label, int selected, IList<string> selectableOptions, params GUILayoutOption[] options)
        {
            return OptionSlider(label, selected, selectableOptions, GUI.skin.label, options);
        }

        /// <summary>
        ///     Slider to select between the given options
        /// </summary>
        public static int OptionSlider(GUIContent label, int selected, IList<string> selectableOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            if (style == null) style = GUI.skin.textField;
            var totalPos = GetSliderRect(label, style, options);
            var sliderFieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);

            selected = Mathf.RoundToInt(GUI.HorizontalSlider(GetSliderRect(sliderFieldPos), selected, 0, selectableOptions.Count() - 1));
            GUI.Label(GetSliderFieldRect(sliderFieldPos), selectableOptions[selected]);
            return selected;
        }


        /// <summary>
        ///     Slider to select from a set range of powers for a given base value.
        ///     Operates on the final value, rounds it to the next power and displays it.
        /// </summary>
        public static int MathPowerSlider(GUIContent label, int baseValue, int value, int minPow, int maxPow, params GUILayoutOption[] options)
        {
            var power = (int)Math.Floor(Math.Log(value) / Math.Log(baseValue));
            power = MathPowerSliderRaw(label, baseValue, power, minPow, maxPow, options);
            return (int)Math.Pow(baseValue, power);
        }

        /// <summary>
        ///     Slider to select from a set range of powers for a given base value.
        ///     Operates on the raw power but displays the final calculated value.
        /// </summary>
        public static int MathPowerSliderRaw(GUIContent label, int baseValue, int power, int minPow, int maxPow, params GUILayoutOption[] options)
        {
            var totalPos = GetSliderRect(label, GUI.skin.label, options);
            var sliderFieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);

            power = Mathf.RoundToInt(GUI.HorizontalSlider(GetSliderRect(sliderFieldPos), power, minPow, maxPow));
            GUI.Label(GetSliderFieldRect(sliderFieldPos), Mathf.Pow(baseValue, power).ToString());
            return power;
        }

        #endregion

        #region Int Fields and Slider Wrappers

        /// <summary>
        ///     An integer slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with a label prefixed and an additional int field thereafter if desired.
        /// </summary>
        public static int IntSlider(string label, int value, int minValue, int maxValue, params GUILayoutOption[] options)
        {
            return (int)Slider(new GUIContent(label), value, minValue, maxValue, options);
        }

        /// <summary>
        ///     An integer slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with a label prefixed and an additional int field thereafter if desired.
        /// </summary>
        public static int IntSlider(GUIContent label, int value, int minValue, int maxValue, params GUILayoutOption[] options)
        {
            return (int)Slider(label, value, minValue, maxValue, options);
        }

        /// <summary>
        ///     An integer slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with a label prefixed and an additional int field thereafter if desired.
        /// </summary>
        public static int IntSlider(int value, int minValue, int maxValue, params GUILayoutOption[] options)
        {
            return (int)Slider(GUIContent.none, value, minValue, maxValue, options);
        }

        /// <summary>
        ///     Int Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.IntField, besides the label slide
        ///     field
        /// </summary>
        public static int IntField(string label, int value, Action onChange, params GUILayoutOption[] options)
        {
            return (int)FloatField(new GUIContent(label), value, onChange, options);
        }

        /// <summary>
        ///     Int Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.IntField, besides the label slide
        ///     field
        /// </summary>
        public static int IntField(GUIContent label, int value, Action onChange, params GUILayoutOption[] options)
        {
            return (int)FloatField(label, value, onChange, options);
        }

        /// <summary>
        ///     Int Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.IntField
        /// </summary>
        public static int IntField(int value, Action onChange, params GUILayoutOption[] options)
        {
            return (int)FloatField(value, onChange, options);
        }

        #endregion

        #region Float Slider

        /// <summary>
        ///     A slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with an additional float field thereafter.
        /// </summary>
        public static float Slider(float value, float minValue, float maxValue, params GUILayoutOption[] options)
        {
            return Slider(GUIContent.none, value, minValue, maxValue, options);
        }

        /// <summary>
        ///     A slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with a label prefixed and an additional float field thereafter if desired.
        /// </summary>
        public static float Slider(string label, float value, float minValue, float maxValue, params GUILayoutOption[] options)
        {
            return Slider(new GUIContent(label), value, minValue, maxValue, options);
        }

        /// <summary>
        ///     A slider that emulates the EditorGUILayout version.
        ///     HorizontalSlider with a label prefixed and an additional float field thereafter if desired.
        /// </summary>
        public static float Slider(GUIContent label, float value, float minValue, float maxValue, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.Slider(label, value, minValue, maxValue, options);
#endif

            var totalPos = GetSliderRect(label, GUI.skin.label, options);
            var sliderFieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);

            value = GUI.HorizontalSlider(GetSliderRect(sliderFieldPos), value, minValue, maxValue);
            value = Mathf.Min(maxValue, Mathf.Max(minValue, FloatField(GetSliderFieldRect(sliderFieldPos), value, null)));
            return value;
        }

        #endregion

        #region Float Field

        private static int activeFloatField = -1;
        private static float activeFloatFieldLastValue;
        private static string activeFloatFieldString = "";

        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField, besides the label
        ///     slide field
        /// </summary>
        public static float FloatField(string label, float value, Action onChange, params GUILayoutOption[] options)
        {
            return FloatField(new GUIContent(label), value, onChange, options);
        }

        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField, besides the label
        ///     slide field
        /// </summary>
        public static float FloatField(GUIContent label, float value, Action onChange, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
            {
                var newvalue = EditorGUILayout.FloatField(label, value, options);
                if (newvalue != value)
                    onChange();
                return newvalue;
            }
#endif

            var totalPos = GetFieldRect(label, GUI.skin.label, options);
            var fieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);
            return FloatField(fieldPos, value, onChange);
        }


        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField, besides the label
        ///     slide field
        /// </summary>
        public static float FloatFieldDynamic(GUIContent label, dynamic value, Action onChange, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
            {
                var newvalue = EditorGUILayout.FloatField(label, value, options);
                if (newvalue != value)
                    onChange();
                return newvalue;
            }
#endif

            var totalPos = GetFieldRect(label, GUI.skin.label, options);
            var fieldPos = PrefixLabel(totalPos, 0.5f, label, GUI.skin.label);
            return DynamicField(fieldPos, value, onChange);
        }


        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
        /// </summary>
        public static float DynamicField(Rect pos, dynamic value, Action onChange)
        {
            var originalValue = value;
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUI.FloatField(pos, value);
#endif

            var floatFieldID = GUIUtility.GetControlID("FloatField".GetHashCode(), FocusType.Keyboard, pos) + 1;
            if (floatFieldID == 0)
                return default;

            var recorded = activeFloatField == floatFieldID;
            var active = floatFieldID == GUIUtility.keyboardControl;

            if (active && recorded && activeFloatFieldLastValue != value)
            {
                // Value has been modified externally
                activeFloatFieldLastValue = value;
                activeFloatFieldString = value.ToString();
            }

            // Get stored string for the text field if this one is recorded
            string str = recorded ? activeFloatFieldString : value.ToString();

            // Handle custom copy-paste
            str = HandleCopyPaste(floatFieldID) ?? str;

            var strValue = GUI.TextField(pos, str);
            if (recorded)
                activeFloatFieldString = strValue;

            // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
            var parsed = true;
            if (strValue == "")
            {
                value = activeFloatFieldLastValue = 0;
            }
            else if (strValue != value.ToString())
            {
                float newValue;
                parsed = float.TryParse(strValue, out newValue);
                if (parsed)
                    value = activeFloatFieldLastValue = newValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFloatField = floatFieldID;
                activeFloatFieldString = strValue;
                activeFloatFieldLastValue = value;
            }
            else if (!active && recorded)
            {
                // Lost focus this frame
                activeFloatField = -1;
                if (!parsed)
                    value = strValue.ForceParse();
            }

            if (originalValue != value) onChange();
            return value;
        }

        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
        /// </summary>
        public static float FloatField(float value, Action onChange, params GUILayoutOption[] options)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.FloatField(value, options);
#endif

            var pos = GetFieldRect(GUIContent.none, null, options);
            return FloatField(pos, value, onChange);
        }

        /// <summary>
        ///     Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
        /// </summary>
        public static float FloatField(Rect pos, float value, Action onChange)
        {
            var originalValue = value;
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUI.FloatField(pos, value);
#endif

            var floatFieldID = GUIUtility.GetControlID("FloatField".GetHashCode(), FocusType.Keyboard, pos) + 1;
            if (floatFieldID == 0)
                return value;

            var recorded = activeFloatField == floatFieldID;
            var active = floatFieldID == GUIUtility.keyboardControl;

            if (active && recorded && activeFloatFieldLastValue != value)
            {
                // Value has been modified externally
                activeFloatFieldLastValue = value;
                activeFloatFieldString = value.ToString();
            }

            // Get stored string for the text field if this one is recorded
            var str = recorded ? activeFloatFieldString : value.ToString();

            // Handle custom copy-paste
            str = HandleCopyPaste(floatFieldID) ?? str;

            var strValue = GUI.TextField(pos, str);
            if (recorded)
                activeFloatFieldString = strValue;

            // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
            var parsed = true;
            if (strValue == "")
            {
                value = activeFloatFieldLastValue = 0;
            }
            else if (strValue != value.ToString())
            {
                float newValue;
                parsed = float.TryParse(strValue, out newValue);
                if (parsed)
                    value = activeFloatFieldLastValue = newValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFloatField = floatFieldID;
                activeFloatFieldString = strValue;
                activeFloatFieldLastValue = value;
            }
            else if (!active && recorded)
            {
                // Lost focus this frame
                activeFloatField = -1;
                if (!parsed)
                    value = strValue.ForceParse();
            }

            if (originalValue != value) onChange();
            return value;
        }

        /// <summary>
        ///     Forces to parse to float by cleaning string if necessary
        /// </summary>
        public static float ForceParse(this string str)
        {
            // try parse
            float value;
            if (float.TryParse(str, out value))
                return value;

            // Clean string if it could not be parsed
            var recordedDecimalPoint = false;
            var strVal = new List<char>(str);
            for (var cnt = 0; cnt < strVal.Count; cnt++)
            {
                var type = CharUnicodeInfo.GetUnicodeCategory(str[cnt]);
                if (type != UnicodeCategory.DecimalDigitNumber)
                {
                    strVal.RemoveRange(cnt, strVal.Count - cnt);
                    break;
                }

                if (str[cnt] == '.')
                {
                    if (recordedDecimalPoint)
                    {
                        strVal.RemoveRange(cnt, strVal.Count - cnt);
                        break;
                    }

                    recordedDecimalPoint = true;
                }
            }

            // Parse again
            if (strVal.Count == 0)
                return 0;
            str = new string(strVal.ToArray());
            if (!float.TryParse(str, out value))
                Debug.LogError("Could not parse " + str);
            return value;
        }

        /// <summary>
        ///     Add copy-paste functionality to any text field
        ///     Returns changed text or NULL.
        ///     Usage: text = HandleCopyPaste (controlID) ?? text;
        /// </summary>
        public static string HandleCopyPaste(int controlID)
        {
            if (controlID == GUIUtility.keyboardControl)
                if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
                {
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        Event.current.Use();
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.Copy();
                    }
                    else if (Event.current.keyCode == KeyCode.V)
                    {
                        Event.current.Use();
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.Paste();
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                        return editor.text;
#else
						return editor.content.text;
#endif
                    }
                    else if (Event.current.keyCode == KeyCode.A)
                    {
                        Event.current.Use();
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.SelectAll();
                    }
                }

            return null;
        }

        #endregion

        #endregion

        #region Object Field

        /// <summary>
        ///     Provides an object field both for editor (using default) and for runtime (not yet implemented other that displaying
        ///     object)
        /// </summary>
        public static T ObjectField<T>(T obj, bool allowSceneObjects) where T : Object
        {
            return ObjectField(GUIContent.none, obj, allowSceneObjects);
        }

        /// <summary>
        ///     Provides an object field both for editor (using default) and for runtime (not yet implemented other that displaying
        ///     object)
        /// </summary>
        public static T ObjectField<T>(string label, T obj, bool allowSceneObjects) where T : Object
        {
            return ObjectField(new GUIContent(label), obj, allowSceneObjects);
        }

        /// <summary>
        ///     Provides an object field both for editor (using default) and for runtime (not yet implemented other that displaying
        ///     object)
        /// </summary>
        public static T ObjectField<T>(GUIContent label, T obj, bool allowSceneObjects, params GUILayoutOption[] options) where T : Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects) as T;
#endif

            var open = false;
            if (obj.GetType() == typeof(Texture2D))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label);
                open = GUILayout.Button(obj as Texture2D, GUILayout.MaxWidth(64), GUILayout.MaxHeight(64));
                GUILayout.EndHorizontal();
            }
            else
            {
                var style = new GUIStyle(GUI.skin.box);
                open = GUILayout.Button(label, style);
            }

            if (open)
            {
                //Debug.Log ("Selecting Object!");
            }

            return obj;
        }

        #endregion

        #region Popups

        // TODO: Implement RT Popup

        public static Enum EnumPopup(Enum selected)
        {
            return EnumPopup(GUIContent.none, selected);
        }

        public static Enum EnumPopup(string label, Enum selected)
        {
            return EnumPopup(new GUIContent(label), selected);
        }

        public static Enum EnumPopup(GUIContent label, Enum selected)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || NodeEditorGUI.isEditorWindow)
                return EditorGUILayout.EnumPopup(label, selected);
#endif

            label = new GUIContent(label);
            label.text += ": " + selected;
            GUILayout.Label(label);
            return selected;
        }

        public static int Popup(GUIContent label, int selected, string[] displayedOptions)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label);
                selected = EditorGUILayout.Popup(selected, displayedOptions);
                GUILayout.EndHorizontal();
                return selected;
            }
#endif

            GUILayout.BeginHorizontal();
            label.text += ": " + selected;
            GUILayout.Label(label);
            GUILayout.EndHorizontal();
            return selected;
        }

        public static int Popup(string label, int selected, string[] displayedOptions)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return EditorGUILayout.Popup(label, selected, displayedOptions);
#endif
            GUILayout.Label(label + ": " + selected);
            return selected;
        }

        public static int Popup(int selected, string[] displayedOptions)
        {
            return Popup("", selected, displayedOptions);
        }

        #endregion

        #region Low-Level Drawing

        private static Material lineMaterial;
        private static Texture2D lineTexture;

        private static void SetupLineMat(Texture tex, Color col)
        {
            if (lineMaterial == null)
            {
                var lineShader = ResourceManager.ShaderLoader?.Invoke("Hidden/LineShader") ?? Shader.Find("Hidden/LineShader");
                if (lineShader == null)
                    throw new NotImplementedException("Missing line shader implementation!");
                lineMaterial = new Material(lineShader);
            }

            if (tex == null)
                tex = lineTexture != null ? lineTexture : lineTexture = ResourceManager.LoadTexture("Textures/AALine.png");
            lineMaterial.SetTexture("_LineTexture", tex);
            lineMaterial.SetColor("_LineColor", col);
            lineMaterial.SetPass(0);
        }

        /// <summary>
        ///     Draws a Bezier curve just as UnityEditor.Handles.DrawBezier, non-clipped. If width is 1, tex is ignored; Else if
        ///     tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored and tex is used.
        /// </summary>
        public static void DrawBezier(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color col, Texture2D tex, float width = 1)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var clippingRect = GUIScaleUtility.getTopRect;
            clippingRect.x = clippingRect.y = 0;
            var bounds = new Rect(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y),
                Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.y - endPos.y));
            if (!bounds.Overlaps(clippingRect))
                return;

            // Own Bezier Formula
            // Slower than handles because of the horrendous amount of calls into the native api

            // Calculate optimal segment count
            var segmentCount = CalculateBezierSegmentCount(startPos, endPos, startTan, endTan);
            // Draw bezier with calculated segment count
            DrawBezier(startPos, endPos, startTan, endTan, col, tex, segmentCount, width);
        }

        /// <summary>
        ///     Draws a clipped Bezier curve just as UnityEditor.Handles.DrawBezier.
        ///     If width is 1, tex is ignored; Else if tex is null, a anti-aliased texture tinted with col will be used; else, col
        ///     is ignored and tex is used.
        /// </summary>
        public static void DrawBezier(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color col, Texture2D tex, int segmentCount, float width)
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
                return;

            var clippingRect = GUIScaleUtility.getTopRect;
            clippingRect.x = clippingRect.y = 0;
            var bounds = new Rect(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y),
                Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.y - endPos.y));
            if (!bounds.Overlaps(clippingRect))
                return;

            // Own Bezier Formula
            // Slower than handles because of the horrendous amount of calls into the native api

            // Calculate bezier points
            var bezierPoints = new Vector2[segmentCount + 1];
            for (var pointCnt = 0; pointCnt <= segmentCount; pointCnt++)
                bezierPoints[pointCnt] = GetBezierPoint((float)pointCnt / segmentCount, startPos, endPos, startTan, endTan);
            // Draw polygon line from the bezier points
            DrawPolygonLine(bezierPoints, col, tex, width);
        }

        /// <summary>
        ///     Draws a clipped polygon line from the given points.
        ///     If width is 1, tex is ignored; Else if tex is null, a anti-aliased texture tinted with col will be used; else, col
        ///     is ignored and tex is used.
        /// </summary>
        public static void DrawPolygonLine(Vector2[] points, Color col, Texture2D tex, float width = 1)
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
                return;

            // Simplify basic cases
            if (points.Length == 1)
                return;
            if (points.Length == 2)
                DrawLine(points[0], points[1], col, tex, width);

            // Setup for drawing
            SetupLineMat(tex, col);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Color.white);

            // Fetch clipping rect
            var clippingRect = GUIScaleUtility.getTopRect;
            clippingRect.x = clippingRect.y = 0;

            Vector2 curPoint = points[0], nextPoint, perpendicular;
            bool clippedP0, clippedP1;
            for (var pointCnt = 1; pointCnt < points.Length; pointCnt++)
            {
                nextPoint = points[pointCnt];

                // Clipping test
                Vector2 curPointOriginal = curPoint, nextPointOriginal = nextPoint;
                if (SegmentRectIntersection(clippingRect, ref curPoint, ref nextPoint, out clippedP0, out clippedP1))
                {
                    // (partially) visible
                    // Calculate apropriate perpendicular
                    if (pointCnt < points.Length - 1) // Interpolate perpendicular inbetween the point chain
                        perpendicular = CalculatePointPerpendicular(curPointOriginal, nextPointOriginal, points[pointCnt + 1]);
                    else // At the end, there's no further point to interpolate the perpendicular from
                        perpendicular = CalculateLinePerpendicular(curPointOriginal, nextPointOriginal);

                    if (clippedP0)
                    {
                        // Just became visible, so enable GL again and draw the clipped line start point
                        GL.End();
                        GL.Begin(GL.TRIANGLE_STRIP);
                        DrawLineSegment(curPoint, perpendicular * width / 2);
                    }

                    // Draw first point before starting with the point chain. Placed here instead of before because of clipping
                    if (pointCnt == 1)
                        DrawLineSegment(curPoint, CalculateLinePerpendicular(curPoint, nextPoint) * width / 2);
                    // Draw the actual point
                    DrawLineSegment(nextPoint, perpendicular * width / 2);
                }
                else if (clippedP1)
                {
                    // Just became invisible, so disable GL
                    GL.End();
                    GL.Begin(GL.TRIANGLE_STRIP);
                }

                // Update state variable
                curPoint = nextPointOriginal;
            }

            // Finalize drawing
            GL.End();
        }

        /// <summary>
        ///     Calculates the optimal bezier segment count for the given bezier
        /// </summary>
        private static int CalculateBezierSegmentCount(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan)
        {
            var straightFactor = Vector2.Angle(startTan - startPos, endPos - startPos) * Vector2.Angle(endTan - endPos, startPos - endPos) * (endTan.magnitude + startTan.magnitude);
            straightFactor = 2 + Mathf.Pow(straightFactor / 400, 0.125f); // 1/8
            var distanceFactor = 1 + (startPos - endPos).magnitude;
            distanceFactor = Mathf.Pow(distanceFactor, 0.25f); // 1/4
            return 4 + (int)(straightFactor * distanceFactor);
        }

        /// <summary>
        ///     Calculates the normalized perpendicular vector of the give line
        /// </summary>
        private static Vector2 CalculateLinePerpendicular(Vector2 startPos, Vector2 endPos)
        {
            return new Vector2(endPos.y - startPos.y, startPos.x - endPos.x).normalized;
        }

        /// <summary>
        ///     Calculates the normalized perpendicular vector for the pointPos interpolated with its two neighbours prevPos and
        ///     nextPos
        /// </summary>
        private static Vector2 CalculatePointPerpendicular(Vector2 prevPos, Vector2 pointPos, Vector2 nextPos)
        {
            return CalculateLinePerpendicular(pointPos, pointPos + (nextPos - prevPos));
        }

        /// <summary>
        ///     Gets the point of the bezier at t
        /// </summary>
        private static Vector2 GetBezierPoint(float t, Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan)
        {
            var rt = 1 - t;
            var rtt = rt * t;

            return startPos * rt * rt * rt +
                   startTan * 3 * rt * rtt +
                   endTan * 3 * rtt * t +
                   endPos * t * t * t;
        }

        /// <summary>
        ///     Adds a line sgement to the GL buffer. Useed in a row to create a line
        /// </summary>
        private static void DrawLineSegment(Vector2 point, Vector2 perpendicular)
        {
            GL.TexCoord2(0, 0);
            GL.Vertex(point - perpendicular);
            GL.TexCoord2(0, 1);
            GL.Vertex(point + perpendicular);
        }

        /// <summary>
        ///     Draws a non-clipped line. If tex is null, a anti-aliased texture tinted with col will be used; else, col is ignored
        ///     and tex is used.
        /// </summary>
        public static void DrawLine(Vector2 startPos, Vector2 endPos, Color col, Texture2D tex, float width = 1)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            // Setup
            SetupLineMat(tex, col);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Color.white);
            // Fetch clipping rect
            var clippingRect = GUIScaleUtility.getTopRect;
            clippingRect.x = clippingRect.y = 0;
            // Clip to rect
            if (SegmentRectIntersection(clippingRect, ref startPos, ref endPos))
            {
                // Draw with clipped line if it is visible
                var perpWidthOffset = CalculateLinePerpendicular(startPos, endPos) * width / 2;
                DrawLineSegment(startPos, perpWidthOffset);
                DrawLineSegment(endPos, perpWidthOffset);
            }

            // Finalize drawing
            GL.End();
        }

        /// <summary>
        ///     Clips the line between the points p1 and p2 to the bounds rect.
        ///     Uses Liang-Barsky Line Clipping Algorithm.
        /// </summary>
        private static bool SegmentRectIntersection(Rect bounds, ref Vector2 p0, ref Vector2 p1)
        {
            bool cP0, cP1;
            return SegmentRectIntersection(bounds, ref p0, ref p1, out cP0, out cP1);
        }


        /// <summary>
        ///     Clips the line between the points p1 and p2 to the bounds rect.
        ///     Uses Liang-Barsky Line Clipping Algorithm.
        /// </summary>
        private static bool SegmentRectIntersection(Rect bounds, ref Vector2 p0, ref Vector2 p1, out bool clippedP0, out bool clippedP1)
        {
            var t0 = 0.0f;
            var t1 = 1.0f;
            var dx = p1.x - p0.x;
            var dy = p1.y - p0.y;

            if (ClipTest(-dx, p0.x - bounds.xMin, ref t0, ref t1)) // Left
                if (ClipTest(dx, bounds.xMax - p0.x, ref t0, ref t1)) // Right
                    if (ClipTest(-dy, p0.y - bounds.yMin, ref t0, ref t1)) // Bottom
                        if (ClipTest(dy, bounds.yMax - p0.y, ref t0, ref t1)) // Top
                        {
                            clippedP0 = t0 > 0;
                            clippedP1 = t1 < 1;

                            if (clippedP1)
                            {
                                p1.x = p0.x + t1 * dx;
                                p1.y = p0.y + t1 * dy;
                            }

                            if (clippedP0)
                            {
                                p0.x = p0.x + t0 * dx;
                                p0.y = p0.y + t0 * dy;
                            }

                            return true;
                        }

            clippedP1 = clippedP0 = true;
            return false;
        }

        /// <summary>
        ///     Liang-Barsky Line Clipping Test
        /// </summary>
        private static bool ClipTest(float p, float q, ref float t0, ref float t1)
        {
            var u = q / p;

            if (p < 0.0f)
            {
                if (u > t1)
                    return false;
                if (u > t0)
                    t0 = u;
            }
            else if (p > 0.0f)
            {
                if (u < t0)
                    return false;
                if (u < t1)
                    t1 = u;
            }
            else if (q < 0.0f)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Texture Utilities

        /// <summary>
        ///     Create a 1x1 tex with color col
        /// </summary>
        public static Texture2D ColorToTex(int pxSize, Color col)
        {
            var texCols = new Color[pxSize * pxSize];
            for (var px = 0; px < pxSize * pxSize; px++)
                texCols[px] = col;
            var tex = new Texture2D(pxSize, pxSize);
            tex.SetPixels(texCols);
            tex.Apply();
            return tex;
        }

        /// <summary>
        ///     Tint the texture with the color. It's advised to use ResourceManager.GetTintedTexture to account for doubles.
        /// </summary>
        public static Texture2D Tint(Texture2D tex, Color color)
        {
            var tintedTex = Object.Instantiate(tex);
            for (var x = 0; x < tex.width; x++)
            for (var y = 0; y < tex.height; y++)
                tintedTex.SetPixel(x, y, tex.GetPixel(x, y) * color);
            tintedTex.Apply();
            return tintedTex;
        }

        /// <summary>
        ///     Rotates the texture Counter-Clockwise, 'quarterSteps' specifying the times
        /// </summary>
        public static Texture2D RotateTextureCCW(Texture2D tex, int quarterSteps)
        {
            if (tex == null)
                return null;
            // Copy and setup working arrays
            tex = Object.Instantiate(tex);
            int width = tex.width, height = tex.height;
            var col = tex.GetPixels();
            var rotatedCol = new Color[width * height];
            for (var itCnt = 0; itCnt < quarterSteps; itCnt++)
            {
                // For each iteration, perform rotation of 90 degrees
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    rotatedCol[x * width + y] = col[(width - y - 1) * width + x];
                rotatedCol.CopyTo(col, 0); // Push rotation for next iteration
            }

            // Apply rotated working arrays
            tex.SetPixels(col);
            tex.Apply();
            return tex;
        }

        /// <summary>
        ///     Rotates the texture Clockwise, 'quarterSteps' specifying the number of 90-degree rotations.
        /// </summary>
        public static Texture2D RotateTextureCW(Texture2D tex, int quarterSteps)
        {
            if (tex == null)
                return null;

            // Copy and setup working arrays
            tex = Object.Instantiate(tex);
            int width = tex.width, height = tex.height;
            var col = tex.GetPixels();
            var rotatedCol = new Color[width * height];

            for (var itCnt = 0; itCnt < quarterSteps; itCnt++)
            {
                // For each iteration, perform rotation of 90 degrees clockwise
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    rotatedCol[x + y * width] = col[height - y - 1 + x * height];
                rotatedCol.CopyTo(col, 0); // Push rotation for next iteration
            }

            // Apply rotated working arrays
            tex.SetPixels(col);
            tex.Apply();
            return tex;
        }

        #endregion
    }
}