using System;
using System.Collections.Generic;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.SubCircuit
{
    public class SubCircuitFactory
    {
        public static IRule Create(Circuit circuit)
        {
            Type genericType = typeof(SubcircuitRule<>).MakeGenericType(circuit.GetFirstOutput().OutputType);
            // Create an instance of the constructed generic type Port<T>
            var ret = (IRule)Activator.CreateInstance(genericType);
            return ret;
        }
    }
}
