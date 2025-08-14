using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.External
{
    [RuleMenu(Path = "Input/External/Long")]
    public class LongInputRule : ExternalRuleBase<long>
    {
        public LongInputRule()
        {
            RuleName = "Hal.LongEx";
            RuleType = RuleType.External;
        }
    }
}