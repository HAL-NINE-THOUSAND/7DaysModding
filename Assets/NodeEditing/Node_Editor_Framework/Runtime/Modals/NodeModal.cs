using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Modals
{
    public abstract class NodeModal
    {
        public Rect ModalSize;
        public abstract void OnShow();
        
        public abstract void OnClose();
        public abstract void Draw();
    }
}