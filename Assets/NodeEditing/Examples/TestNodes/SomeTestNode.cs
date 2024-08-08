using System;
using NodeEditing.Examples.FloatCalculation;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NodeEditing.Examples.TestNodes
{
    public class SomeTestNode : Node
	{
		public const string ID = nameof(SomeTestNode);
		public override string GetID { get { return ID; } }

		public override string Title { get { return "Something/A Test"; } }
		public override Vector2 DefaultSize { get { return new Vector2 (200, 100); } }


		public IPort Input1;
		public IPort Input2;

		public IPort Output1;

		private void OnEnable()
		{
			
			Input1 = Port<int>.Create("Input 1", Direction.In, this, (int)(Random.value * 100));
			Input2 = Port<int>.Create("Input 2", Direction.In, this, (int)(Random.value * 100));

			Output1 = Port<int>.Create("Output 1", Direction.Out, this, 0);
			IncomingPorts.Add(Input1);
			IncomingPorts.Add(Input2);
			OutgoingPorts.Add(Output1);
			
		}

		public override void NodeGUI () 
		{
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();

			
			var input1Value = Input1.Connected ? Input1.Connections[0].SourcePort.GetValue<int>() : Input1.GetValue<int>();
			var input2Value = Input2.Connected ? Input2.Connections[0].SourcePort.GetValue<int>() : Input2.GetValue<int>(); 

			// First input
			if (Input1.Connected)
				GUILayout.Label (Input1.Knob.name + ": " + input1Value);
			else
				Input1.SetValue(RTEditorGUI.IntField(GUIContent.none, Input1.GetValue<int>()));
			Input1.Knob.SetPosition ();


			// Second input
			if (Input2.Connected)
				GUILayout.Label (Input2.Knob.name + ": " + input2Value);
			else
				Input2.SetValue(RTEditorGUI.IntField(GUIContent.none, Input2.GetValue<int>()));
			Input2.Knob.SetPosition ();

			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();

			// Output
			Output1.Knob.DisplayLayout ();

			for (int i = 0; i < OutgoingPorts.Count; i++)
				OutgoingPorts[i].DisplayLayout ();
			
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();

			//type = (CalcNode.CalcType)RTEditorGUI.EnumPopup (new GUIContent ("Calculation Type", "The type of calculation performed on Input 1 and Input 2"), type);

			if (GUI.changed)
				NodeEditor.curNodeCanvas.OnNodeChange (this);
		}

		public override bool Calculate ()
		{
			int a=0;
			int b = 0;
			if (Input1.Connected)
				a = Input1.Connections[0].SourcePort.GetValue<int>();
			else
				a = Input1.GetValue<int>();
			
			if (Input2.Connected)
				b = Input2.Connections[0].SourcePort.GetValue<int>();
			else
				b = Input2.GetValue<int>();

			Output1.SetValue<int>(a + b);
			
			// switch (type) 
			// {
			// case CalcNode.CalcType.Add:
			// 	outputKnob.SetValue<float> (Input1Val + Input2Val);
			// 	break;
			// case CalcNode.CalcType.Substract:
			// 	outputKnob.SetValue<float> (Input1Val - Input2Val);
			// 	break;
			// case CalcNode.CalcType.Multiply:
			// 	outputKnob.SetValue<float> (Input1Val * Input2Val);
			// 	break;
			// case CalcNode.CalcType.Divide:
			// 	outputKnob.SetValue<float> (Input1Val / Input2Val);
			// 	break;
			// }

			return true;
		}
	}
}