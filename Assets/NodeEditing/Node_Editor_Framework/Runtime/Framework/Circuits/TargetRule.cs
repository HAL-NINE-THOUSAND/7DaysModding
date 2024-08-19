namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public abstract class TargetRule<TOutput, TTarget> : Rule<TOutput>
    {
        public TTarget Target { get; set; }
    }
}