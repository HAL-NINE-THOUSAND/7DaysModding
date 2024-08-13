using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
using MenuFunctionData = UnityEditor.GenericMenu.MenuFunction2;
#else
using MenuFunction = NodeEditorFramework.Utilities.OverlayGUI.CustomMenuFunction;
using MenuFunctionData = NodeEditorFramework.Utilities.OverlayGUI.CustomMenuFunctionData;
#endif

namespace NodeEditorFramework.Utilities
{
    public class MenuHelper
    {

        public static GenericMenu CreateMenu(GenericMenu root, IEnumerable<string> items)
        {
            MenuFunctionData menuFunction = (object callbackObj) => 
            {
                // if (!(callbackObj is NodeEditorInputInfo))
                //     throw new UnityException ("Callback Object passed by context is not of type NodeEditorMenuCallback!");
                //actionDelegate.DynamicInvoke (callbackObj as NodeEditorInputInfo);
                Debug.Log("GOT HERE: " + callbackObj);
            };
            foreach (var item in items)
            {
                root.AddItem(new GUIContent(item), true, menuFunction, item);
            }

            return root;
        }
        
    }
}