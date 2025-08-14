using System;
using System.Collections.Generic;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.SubCircuit
{
    public interface ISubcircuit
    {
        void SetupCircuit(Guid circuitId);
        Guid CircuitId { get; set; }
    }
    public class SubcircuitRule<T> : Rule<T>, ISubcircuit
    {
        public const string Identifier = "Hal.Subcircuit";

        public Circuit Subcircuit;
        public Guid CircuitId { get; set; }

        private bool isInitialised = false;

        private Dictionary<Guid, IRule> InputRules = new Dictionary<Guid, IRule>();
        
        public SubcircuitRule()
        {
            RuleName = Identifier;
            RuleType = RuleType.Processor;
            Logic = () =>
            {

                if (!isInitialised)
                {
                    isInitialised = true;
                    SetupCircuit(CircuitId);
                }

                if (Subcircuit != null)
                {
                    Subcircuit.Run();
                    var value = Subcircuit.GetFirstOutputValue<T>(Subcircuit.GetFirstOutput());
                    SetLastValue(value);
                }
                // Circuit.GetValue(Input1, out var value1, out _);
                // Circuit.GetValue(Input2, out var value2, out _);
                //
                // var value =  value1 + value2;
                
                return lastValue;
            };
        }

        public static IRule Create(Circuit circuit)
        {
            Type genericType = typeof(Port<>).MakeGenericType(circuit.GetFirstOutput().OutputType);
            // Create an instance of the constructed generic type Port<T>
            var ret = (IRule)Activator.CreateInstance(genericType);
            return ret;
        }
        
        public void SetupCircuit(Guid circuitId)
        {
            if (!RTNodeEditor.CircuitManager.AllCircuits.TryGetValue(circuitId, out var circuit))
            {
                return;
            }

            Subcircuit = circuit;

            var inputs = Subcircuit.GetExternalInputs();
            foreach (var input in inputs)
            {
                
                var rule = (IRule)input;
                var port = CreatePort(input.VariableName, this, rule.OutputType);
                InputRules.Add(port.InputId, rule);
                Inputs.Add(port);
            }

        }
        
        public sealed override Func<T> Logic { get; set; }
        
        
        
        public static IPort CreatePort(string name, IRule parent, Type type)
        {
            Type genericType = typeof(Port<>).MakeGenericType(type);
            // Create an instance of the constructed generic type Port<T>
            var ret = (IPort)Activator.CreateInstance(genericType, name, parent);

            parent.Inputs.Add(ret);
            return ret;
        }

    }
}
