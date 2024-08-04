using System;
using NodeEditorFramework;
using UnityEngine;

namespace NodeEditing.Examples.FloatCalculation
{
	// public class FloatConnectionType : ValueConnectionType
	// {
	// 	public override string Identifier { get { return "Float"; } }
	// 	public override Color Color { get { return Color.cyan; } }
	// 	public override Type Type { get { return typeof(float); } }
	// }
	//
	//
	public class DefaultConnectionType : ConnectionKnobStyle
	{
		public override string Identifier { get { return "Default"; } }
		public override Color Color { get { return Color.cyan; } }
		//public override Type Type { get { return typeof(float); } }
	}
}
