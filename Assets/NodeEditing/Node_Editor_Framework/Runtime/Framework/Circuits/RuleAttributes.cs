using System;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RuleMenuAttribute : Attribute
    {
        public bool Hidden;
        public string Path;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RuleTitleAttribute : Attribute
    {
        public string Title;
    }
}