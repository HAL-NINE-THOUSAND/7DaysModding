using System;
using System.Diagnostics;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    // Define a generic Port class implementing IPort
    [DebuggerDisplay("Port: {Name}: {Value}")]
    public class Port<T> : IPort
    {
        public Port(string name, IRule parent)
        {
            this.Name = name;
            Rule = parent;
        }

        public string Name { get; }

        public Guid InputId { get; set; } = Guid.NewGuid();

        public IRule Rule { get; set; }

        public Type PortType => typeof(T);

        /// <summary>
        ///     Draws a label with the given GUIContent and the given style. Places the knob next to it at it's nodeSide
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
        IRule Rule { get; set; }
    }
}