using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.External
{
    [RuleMenu(Path = "Input/External/Byte")]
    public class ByteInputRule : ExternalRuleBase<short>
    {
        public ByteInputRule() : base()
        {
            RuleName = "Hal.ByteEx";
            RuleType = RuleType.External;
        }
    }
}