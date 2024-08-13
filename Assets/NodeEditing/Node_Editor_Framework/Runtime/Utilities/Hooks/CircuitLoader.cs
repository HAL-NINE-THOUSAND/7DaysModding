using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Utilities.Hooks
{
    public class CircuitLoader
    {
        public static Action<NodeEditorFramework.Utilities.GenericMenu, NodeCanvas> LoadMenuGenerator;

        public static void TestBuild(GenericMenu menu)
        {
            var items = new List<string>()
            {
                "Load/Test1",
                "Load/Others",
                "Load/Others/Test2",
                "Load/Others/Test3",
                "Load/Test 4",
            };

            foreach (var item in items)
                menu.AddItem(new GUIContent(item), true, data => Debug.Log($"load item: {data}"), item);
        }
    }
}