using System;
using System.Linq;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.SubCircuit;
using NodeEditorFramework;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public class SubTest : MonoBehaviour
    {
        public void Update()
        {

            if (Input.GetKeyUp(KeyCode.F10))
            {

                var circuit = RTNodeEditor.CircuitManager.AllCircuits.Values.FirstOrDefault();
                if (circuit == null)
                    return;

                if (RTNodeEditor.Instance == null)
                    return;

                var nodes = RTNodeEditor.Instance;

                var activeCircuit = nodes.canvasCache.nodeCanvas.Circuit;

                var subcircuit = SubCircuitFactory.Create(circuit);
                ((ISubcircuit)subcircuit).SetupCircuit(circuit.CircuitId);
                Node.Create(subcircuit, nodes.canvasCache.nodeCanvas);
            }
            
        }
    }
}