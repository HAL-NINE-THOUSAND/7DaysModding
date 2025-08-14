using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using UnityEngine;



public interface ICircuitManager
{
    public Dictionary<string, List<Circuit>> PlayerCircuits { get; set; }
    public Dictionary<Guid, Circuit> AllCircuits { get; set; }
    public ulong GetWorldTime();
    public void Load();
    public void Save(Circuit circuit);
    public Circuit LoadCircuit(Guid circuitId);
    public List<Circuit> GetPlayerCircuits();

    public void Init();
    public void CleanUp();
}

