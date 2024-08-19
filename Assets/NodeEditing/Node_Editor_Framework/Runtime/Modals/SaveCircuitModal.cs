using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Modals
{
    public class SaveCircuitModal : NodeModal
    {
        private static NodeCanvas canvas;

        public string CircuitName = "";
        public bool IsCancelled;

        public bool IsComplete;

        private string message = "You can group circuits using / to separate nodes e.g. MyStuff/Entities/KillBob";

        public SaveCircuitModal()
        {
            ModalSize = new Rect(-1, -1, 300, 200);
        }

        public override void OnShow()
        {
            IsComplete = false;
            IsCancelled = false;
        }

        public override void OnClose()
        {
            if (IsCancelled)
                return;

            Debug.Log("Save circuit....");
            canvas.Circuit.Name = CircuitName;
            Circuit.SaveCircuit(canvas.Circuit);
        }

        public static void HandleMenuDraw(GenericMenu menu, NodeCanvas activeCanvas)
        {
            canvas = activeCanvas;
            menu.AddItem(new GUIContent("  Save Circuit"), false, () => ShowModal(canvas));
        }

        public static void ShowModal(NodeCanvas activeCanvas)
        {
            canvas = activeCanvas;
            ModalManager.ShowModal(new SaveCircuitModal
            {
                CircuitName = canvas.Circuit.Name
            });
        }

        public override void Draw()
        {
            GUILayout.Label("Save Circuit");

            // File save field
            GUILayout.BeginVertical();
            GUILayout.Label("Circuit Name", GUILayout.ExpandWidth(false));
            CircuitName = GUILayout.TextField(CircuitName, GUILayout.ExpandWidth(true));
            //GUILayout.Label("." + FormatExtension, GUILayout.ExpandWidth (false));
            GUILayout.EndVertical();

            // Finish operation buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                IsCancelled = true;
                IsComplete = true;
                ModalManager.CloseModal();
                return;
            }

            if (GUILayout.Button("Save"))
            {
                if (string.IsNullOrEmpty(CircuitName))
                {
                    message = "name is invalid. you should really do something about that you know. Or how would you find it again? Riddle me that, user";
                    return;
                }

                if (Circuit.SaveCircuit == null)
                {
                    message = "No save function set so there's not much I can do...";
                    return;
                }

                IsComplete = true;
                ModalManager.CloseModal();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label(message, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
        }
    }
}