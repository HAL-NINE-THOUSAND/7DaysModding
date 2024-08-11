using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Core
{
    public class UI
    {

        public static float LabelHeight(Rect rect, GUIContent content, GUIStyle style)
        {
            GUI.Label(rect, content, style);
            var ret = style.CalcHeight(content, rect.width);
            return ret;
        }
        
    }
}