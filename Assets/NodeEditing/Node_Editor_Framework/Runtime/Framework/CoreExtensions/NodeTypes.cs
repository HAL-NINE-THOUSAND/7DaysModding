﻿using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits;

namespace NodeEditorFramework
{
	/// <summary>
	///     Handles fetching and storing of all Node declarations
	/// </summary>
	public static class NodeTypes
    {
        private static Dictionary<string, NodeTypeData> nodes;
        private static Dictionary<string, NodeTypeData> nodesByRuleId;

        /// <summary>
        ///     Fetches every Node Declaration in the script assemblies to provide the framework with custom node types
        /// </summary>
        public static void FetchNodeTypes()
        {
            nodes = new Dictionary<string, NodeTypeData>();
            nodesByRuleId = new Dictionary<string, NodeTypeData>();

            var rules = RulesManager.GetAllTypes<IRule>();

            foreach (var type in rules)
            {
                var menuData = RulesManager.GetAttribute<RuleMenuAttribute>(type);
                var titleAtt = RulesManager.GetAttribute<RuleTitleAttribute>(type);

                var title = titleAtt?.Title ?? menuData.Path.Split("/").Last();
                var nodeData = new NodeTypeData(menuData.Path, title, type, new Type[0], menuData.Hidden);
                nodes.Add(menuData.Path, nodeData);
                var ruleInstance = RulesManager.CreateRule(type);
                nodesByRuleId.Add(ruleInstance.RuleName, nodeData);
            }

            // foreach (Type type in ReflectionUtility.getSubTypes (typeof(Node)))	
            // {
            // 	object[] nodeAttributes = type.GetCustomAttributes(typeof(NodeAttribute), false);
            // 	NodeAttribute attr = nodeAttributes.FirstOrDefault() as NodeAttribute; //[0] as NodeAttribute;
            // 	if(attr == null || !attr.hide)
            // 	{ // Only regard if it is not marked as hidden
            // 	  // Fetch node information
            // 		string ID, Title = "None";
            // 		FieldInfo IDField = type.GetField("ID");
            // 		if (IDField == null || attr == null)
            // 		{ // Cannot read ID from const field or need to read Title because of missing attribute -> Create sample to read from properties
            // 			Node sample = (Node)ScriptableObject.CreateInstance(type);
            // 			ID = sample.GetID;
            // 			Title = sample.Title;
            // 			UnityEngine.Object.DestroyImmediate(sample);
            // 		}
            // 		else // Can read ID directly from const field
            // 			ID = (string)IDField.GetValue(null);
            // 		// Create Data from information
            // 		NodeTypeData data = attr == null?  // Switch between explicit information by the attribute or node information
            // 			new NodeTypeData(ID, Title, type, new Type[0]) :
            // 			new NodeTypeData(ID, attr.contextText, type, attr.limitToCanvasTypes);
            // 		nodes.Add (ID, data);
            // 	}
            // }
        }

        /// <summary>
        ///     Returns all recorded node definitions found by the system
        /// </summary>
        public static IEnumerable<NodeTypeData> getNodeDefinitions()
        {
            return nodes.Values;
        }

        /// <summary>
        ///     Returns the NodeData for the given node type ID
        /// </summary>
        public static NodeTypeData getNodeData(string typeID)
        {
            NodeTypeData data;
            nodes.TryGetValue(typeID, out data);
            return data;
        }

        /// <summary>
        ///     Returns the NodeData for the given node type ID
        /// </summary>
        public static NodeTypeData getNodeDataByRuleId(string ruleId)
        {
            NodeTypeData data;
            nodesByRuleId.TryGetValue(ruleId, out data);
            return data;
        }

        /// <summary>
        ///     Returns all node IDs that can automatically connect to the specified port.
        ///     If port is null, all node IDs are returned.
        /// </summary>
        public static List<string> getCompatibleNodes(ConnectionPort port)
        {
            if (port == null)
                return nodes.Keys.OrderBy(d => d).ToList();
            var compatibleNodes = new List<string>();
            foreach (var nodeData in nodes.Values)
                // Iterate over all nodes to check compability of any of their connection ports
                if (ConnectionPortManager.GetPortDeclarations(nodeData.typeID).Any(
                        portDecl => portDecl.portInfo.IsCompatibleWith(port)))
                    compatibleNodes.Add(nodeData.typeID);

            compatibleNodes.Sort((a, b) => a.CompareTo(b));
            return compatibleNodes;
        }
    }

	/// <summary>
	///     The NodeData contains the additional, editor specific data of a node type
	/// </summary>
	public struct NodeTypeData
    {
        public string typeID;
        public string adress;
        public bool hidden;
        public Type type;
        public Type[] limitToCanvasTypes;

        public NodeTypeData(string ID, string name, Type nodeType, Type[] limitedCanvasTypes, bool isHidden)
        {
            typeID = ID;
            adress = name;
            type = nodeType;
            limitToCanvasTypes = limitedCanvasTypes;
            hidden = isHidden;
        }
    }

	/// <summary>
	///     The NodeAttribute is used to specify editor specific data for a node type, later stored using a NodeData
	/// </summary>
	public class NodeAttribute : Attribute
    {
        public NodeAttribute(bool HideNode, string ReplacedContextText, params Type[] limitedCanvasTypes)
        {
            hide = HideNode;
            contextText = ReplacedContextText;
            limitToCanvasTypes = limitedCanvasTypes;
        }

        public bool hide { get; private set; }
        public string contextText { get; private set; }
        public Type[] limitToCanvasTypes { get; private set; }
    }
}