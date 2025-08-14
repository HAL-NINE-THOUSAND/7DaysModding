using System;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Rules.Inputs.External
{
    [RuleMenu(Path = "Input/External/Integer")]
    [RuleTitle(Title = "Int")]
    public class IntegerInputRule : ExternalRuleBase<int>
    {
        public IntegerInputRule()
        {
            RuleName = "Hal.IntEx";
            RuleType = RuleType.External;
        }
    }
}