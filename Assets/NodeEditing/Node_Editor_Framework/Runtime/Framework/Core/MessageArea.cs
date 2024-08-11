using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditorFramework;
using UnityEngine;
namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Core
{
    public class MessageArea
    {
        
        private NodeCanvas canvas;

        private Queue<GUIContent> messages = new();

        public int MaxMessages = 4;

        public float PanelHeight = 100f;
        public MessageArea(NodeCanvas parent)
        {
            canvas = parent;
        }

        public void AddMessage(string msg)
        {
            var content = messages.Count < MaxMessages ? new GUIContent("") : messages.Dequeue();
            content.text = $"{DateTime.Now.Hour.ToString().PadLeft(2,'0')}:{DateTime.Now.Minute.ToString().PadLeft(2,'0')}:{DateTime.Now.Second.ToString().PadLeft(2,'0')} - {msg}";
            messages.Enqueue(content);
        }
        public void DrawMessagePanel(Rect canvasRect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
			
            var spacer = 5;
            
            
            Rect rect = new Rect(canvasRect.x, canvasRect.y + canvasRect.height - PanelHeight, canvasRect.width, PanelHeight);

            GUI.BeginGroup(rect, GUIContent.none, NodeEditorGUI.GUIStyles["box"]);

            var pos = new Rect(spacer, spacer, rect.width, 20);
            var labelStyle = NodeEditorGUI.GUIStyles["labelLeft"];

            foreach (var msg in messages.Reverse()) //allocations? not sure on this runtime
            {
                var height = UI.LabelHeight(pos, msg, labelStyle);
                pos.y += height + spacer;
            }

            if (pos.y > PanelHeight)
                PanelHeight = pos.y;
            GUI.EndGroup();

            // // MOUSE POS RECT TEST
            // int inRect = 1; // State 1: Outside of all rects
            // if (rect.Contains(Event.current.mousePosition))
            // 	inRect = 3; // State 3: Inside group rect
            // else
            // {
            // 	Rect clickRect = new Rect(rect.x - minCloseDistance, rect.y - minCloseDistance, rect.width + 2 * minCloseDistance, rect.height + 2 * minCloseDistance);
            // 	if (clickRect.Contains(Event.current.mousePosition))
            // 		inRect = 2; // State 2: Inside extended click rect
            // }
            //
            // return inRect;
        }
    }
}