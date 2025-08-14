using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.External
{
    [RuleMenu(Path = "Input/External/Short")]
    public class ShortInputRule : ExternalRuleBase<short>
    {
        public ShortInputRule()
        {
            RuleName = "Hal.ShortEx";
            RuleType = RuleType.External;
        }
    }
}