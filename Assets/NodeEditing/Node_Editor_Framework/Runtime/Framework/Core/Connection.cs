namespace NodeEditorFramework
{
    public class Connection
    {
        public IPort SourcePort { get; }
        public IPort TargetPort { get; }

        public Connection(IPort sourcePort, IPort targetPort)
        {
            SourcePort = sourcePort;
            TargetPort = targetPort;
        }
        
        
        public void Delete()
        {
            SourcePort.RemoveConnection(this);
            TargetPort.RemoveConnection(this);
        }
    }
}