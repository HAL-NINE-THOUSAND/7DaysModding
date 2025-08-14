using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.External
{

    public interface IExternalRule
    {
        public Guid ExternalRuleId { get; set; }
        public Guid ExternalCircuitId { get; set; }
        public IRule Rule { get; set; }
        public string VariableName { get; set; }
    }
    
    public abstract class ExternalRuleBase<T> : Rule<T>, IExternalRule
    {
        public ExternalRuleBase()
        {
            //RuleName = "Hal";
            RuleType = RuleType.External;
            Logic = () =>
            {
                T value = default;
                if (RTNodeEditor.CircuitManager.AllCircuits.TryGetValue(ExternalCircuitId, out var externalCircuit))
                {
                    if (externalCircuit.Rules.TryGetValue(ExternalCircuitId, out var externalRule))
                    {
                        value = (T)externalRule.GetLastValue();
                    }
                }
                SetLastValue(value);
                return value;
            };
            
            //Input1 = Port<short>.Create("Input 1", this);
        }

        public Guid ExternalRuleId { get; set; }
        public Guid ExternalCircuitId { get; set; }
        public IRule Rule { get; set; }

        public string VariableName { get; set; }
        
       // public Port<short> Input1 { get; set; }
        public sealed override Func<T> Logic { get; set; }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(VariableName);
            writer.Write(ExternalCircuitId.ToString());
            writer.Write(ExternalRuleId.ToString());
        }

        public override void DrawUI()
        {
            VariableName = RTEditorGUI.TextField(new GUIContent("Name", "Variable name"), VariableName);
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            VariableName = reader.ReadString();
            ExternalCircuitId = Guid.Parse(reader.ReadString());
            ExternalRuleId = Guid.Parse(reader.ReadString());
        }
    }
}