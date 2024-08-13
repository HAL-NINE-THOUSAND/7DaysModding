using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Modals
{
    public static class ModalManager
    {

        private static NodeModal modal; // = new SaveCircuitModal();

        public static void ShowModal(NodeModal modalToShow)
        {
            modal = modalToShow;
        }

        public static void CloseModal()
        {
            modal?.OnClose();
            modal = null;
        }
        public static void DrawModalPanel()
        {
            if (modal == null)
                return;

            if (modal.ModalSize.x < 0)
            {
                modal.ModalSize.x = ((float)Screen.width / 2) - (modal.ModalSize.width / 2);
                modal.ModalSize.y = ((float)Screen.height / 2) - (modal.ModalSize.height / 2);
            }
            GUILayout.BeginArea(modal.ModalSize, GUI.skin.box);
            modal.Draw();
            GUILayout.EndArea();
        }

    }
}