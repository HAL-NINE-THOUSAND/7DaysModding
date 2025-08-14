using System;
using System.Collections.Generic;
using System.IO;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;
using UnityEngine;

// public static class CircuitManagerDefault
// {
//     private static ICircuitManager Saver = new CircuitSaver();
//
//     public static Dictionary<string, List<Circuit>> PlayerCircuits => Saver.PlayerCircuits;
//     public static Dictionary<Guid, Circuit> AllCircuits  => Saver.AllCircuits;
//     public static void Load()
//     {
//         Saver.Load();
//     }
//     public static Circuit LoadCircuit(Guid circuitId)
//     {
//         return Saver.LoadCircuit(circuitId);
//     }
//     public static void Save(Circuit circuit)
//     {
//         Saver.Save(circuit);
//     }
// }

public class TestCircuitManager : ICircuitManager
{

    static string savePath = "C:\\!tmp\\Circuits";
    public Dictionary<string, List<Circuit>> PlayerCircuits { get; set; } = new Dictionary<string, List<Circuit>>();
    public Dictionary<Guid, Circuit> AllCircuits { get; set; } = new Dictionary<Guid, Circuit>();

    public ulong GetWorldTime()
    {
        throw new NotImplementedException();
        return 0;
    }

    public void Load()
    {
        var files = Directory.GetFiles(savePath);
            
        PlayerCircuits.Clear();
        AllCircuits.Clear();

        foreach (var filepath in files)
        {
                
            var circuit = new Circuit();
            using (Stream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    try
                    {
                        circuit.Read(reader);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to load circuit: {filepath}. {ex.Message}");   
                    }
                }
            }
               
            //var circuit = Newtonsoft.Json.JsonConvert.DeserializeObject<Circuit>(File.ReadAllText(file));
            if (!PlayerCircuits.ContainsKey(circuit.OwnerId))
            {
                PlayerCircuits.Add(circuit.OwnerId, new List<Circuit>());
            }

            circuit.Build();
            PlayerCircuits[circuit.OwnerId].Add(circuit);
            AllCircuits.Add(circuit.CircuitId, circuit);
        }

    }
    
    public void Save(Circuit circuit)
    {
            
        if (!PlayerCircuits.ContainsKey(circuit.OwnerId))
        {
            PlayerCircuits.Add(circuit.OwnerId, new List<Circuit>());
        }

        var circuits = PlayerCircuits[circuit.OwnerId];
        circuits.RemoveAll(d => d.CircuitId == circuit.CircuitId);
        circuits.Add(circuit);

        var filepath = $"{savePath}/{circuit.CircuitId}.circuit";
        //Log.Out("Saving circuit to: " + filepath);
        // var options = new JsonSerializerSettings()
        // {
        //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        // };
        // var json = Newtonsoft.Json.JsonConvert.SerializeObject(circuit, Formatting.Indented, options);

        if (File.Exists(filepath))
            File.Delete(filepath);

        using (Stream stream = File.Open(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            using (var pooledBinaryWriter = new BinaryWriter(stream))
            {
                circuit.Write(pooledBinaryWriter);
            }
        }

        PlayerCircuits[circuit.OwnerId].Remove(circuit);
        AllCircuits.Remove(circuit.CircuitId);
        PlayerCircuits[circuit.OwnerId].Add(circuit);
        AllCircuits.Add(circuit.CircuitId, circuit);
        //File.WriteAllText(filepath, json);

    }
        
    public Circuit LoadCircuit(Guid circuitId)
    {

        var filepath = $"{savePath}/{circuitId}.circuit";

        var circuit = new Circuit();
        using (Stream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    circuit.Read(reader);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load circuit: {filepath}. {ex.Message} {ex.StackTrace}");
                }
            }
        }

        return circuit;
    }

    public List<Circuit> GetPlayerCircuits()
    {
        return null;
    }

    public void Init()
    {
        
    }

    public void CleanUp()
    {
        
        
    }
}