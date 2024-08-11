using System;
using System.Diagnostics;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    // Define a generic Port class implementing IPort
    [DebuggerDisplay("Port: {name}: {Value}")]
    public class Port<T> : IPort
    {
        public Guid InputId { get; set; } = Guid.NewGuid();

        private string name;
        public string Name => name;

        public IRule Node { get; set; }

        public Type PortType => typeof(T);

        public Port(string name, IRule parent)
        {
            this.name = name;
            Node = parent;
        }

        /// <summary>
        /// Draws a label with the given GUIContent and the given style. Places the knob next to it at it's nodeSide
        /// </summary>
        public void DisplayLayout()
        {
        }

        public static Port<T> Create(string name, IRule parent)
        {
            var ret = new Port<T>(name, parent);
            parent.Inputs.Add(ret);
            return ret;
        }

        public bool IsSamePort(IPort port)
        {

            if (PortType == port.PortType)
                return true;

            if (port.PortType.IsAssignableFrom(PortType))
                return true;

            if (PortType.IsAssignableFrom(port.PortType))
                return true;

            return false;
            //return port?.PortType == typeof(T);
        }

        public bool IsSamePort<TPortType>()
        {

            var other = typeof(TPortType);
            if (PortType == other)
                return true;

            if (other.IsAssignableFrom(PortType))
                return true;

            if (PortType.IsAssignableFrom(other))
                return true;

            return false;
        }
    }

    public interface IPort
    {
        Guid InputId { get; set; }
        Type PortType { get; }
        IRule Node { get; set; }
    }
}