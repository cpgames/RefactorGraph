using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using NodeGraph;
using NodeGraph.Model;
using RefactorGraph.Nodes;
using RefactorGraph.Nodes.Other;
using RefactorGraph.Nodes.PartitionOperations;

namespace RefactorGraph
{
    public static class Utils
    {
        #region Fields
        public static Action refreshAction;
        #endregion

        #region Methods
        public static string GetOrCreateDir()
        {
            var dir = Directory.GetCurrentDirectory();
            var dirSubs = Directory.GetDirectories(dir, "RefactorGraphs");
            DirectoryInfo dirInfo;
            if (dirSubs.Length == 0)
            {
                dirInfo = Directory.CreateDirectory(Path.Combine(dir, "RefactorGraphs"));
            }
            else
            {
                dirInfo = new DirectoryInfo(dirSubs[0]);
            }
            return dirInfo.FullName;
        }

        public static IEnumerable<string> GetGraphFiles()
        {
            var dir = GetOrCreateDir();
            var files = Directory.GetFiles(dir, "*.rgraph");
            return files.Select(Path.GetFileNameWithoutExtension);
        }

        public static string CreateGraphFilePath(string graphName)
        {
            graphName = $"{graphName}.rgraph";
            var dir = GetOrCreateDir();
            return Path.Combine(dir, graphName);
        }

        public static bool RenameFile(string oldGraphName, string newGraphName)
        {
            try
            {
                var oldFilePath = CreateGraphFilePath(oldGraphName);
                var newFilePath = CreateGraphFilePath(newGraphName);
                File.Move(oldFilePath, newFilePath);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Rename failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool Save(FlowChart flowChart)
        {
            try
            {
                var filePath = CreateGraphFilePath(flowChart.Name);
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                settings.NewLineChars = "\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                settings.NewLineOnAttributes = false;
                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    {
                        writer.WriteStartElement("FlowChart");
                        flowChart.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndDocument();

                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static void Load(string graphName, out FlowChart flowChart)
        {
            var filePath = CreateGraphFilePath(graphName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"FlowGraph <{graphName}> not found.", filePath);
            }

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "FlowChart")
                        {
                            var guid = Guid.Parse(reader.GetAttribute("Guid") ?? throw new InvalidOperationException());
                            var type = Type.GetType(reader.GetAttribute("Type") ?? throw new InvalidOperationException());

                            flowChart = NodeGraphManager.FindFlowChart(guid);
                            if (flowChart == null)
                            {
                                flowChart = NodeGraphManager.CreateFlowChart(true, guid, type);
                                flowChart.ReadXml(reader);
                                flowChart.OnDeserialize();
                            }
                            return;
                        }
                    }
                }
            }
            throw new Exception("Failed to find FlowChart element.");
        }

        public static bool Delete(string graphName)
        {
            try
            {
                var flowChart = NodeGraphManager.FlowCharts.Values.FirstOrDefault(x => x.Name == graphName);
                if (flowChart != null)
                {
                    NodeGraphManager.DestroyFlowChart(flowChart.Guid);
                }
                var filePath = CreateGraphFilePath(graphName);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("FlowGraph file not found.", filePath);
                }

                File.Delete(filePath);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool ValidateGraph(FlowChart flowChart, out StartNode startNode)
        {
            startNode = null;

            var getDocumentNodes = NodeGraphManager.FindNode(flowChart, "Start");
            if (getDocumentNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(flowChart, "You need to place a 'Start' node.");
                return false;
            }
            if (getDocumentNodes.Count > 1)
            {
                NodeGraphManager.AddScreenLog(flowChart, "Only single 'Start' must exist.");
                return false;
            }

            startNode = getDocumentNodes[0] as StartNode;
            if (startNode == null)
            {
                NodeGraphManager.AddScreenLog(flowChart, "You need to place a 'Start' node.");
                return false;
            }
            return true;
        }

        public static IEnumerable<Type> FindAllDerivedTypes(this Type type, Assembly assembly)
        {
            var derivedType = type;
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t));
        }

        public static IEnumerable<Type> FindAllDerivedTypes(this Type type)
        {
            var assembly = Assembly.GetAssembly(type);
            return type.FindAllDerivedTypes(assembly);
        }

        public static T GetAttribute<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return (T)type.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetAttribute<T>() != null;
        }

        public static Type GetNodeType(RefactorNodeType nodeType)
        {
            var types = typeof(RefactorNodeBase)
                .FindAllDerivedTypes()
                .Where(x => x.HasAttribute<RefactorNodeAttribute>());
            return types.FirstOrDefault(x => x.GetAttribute<RefactorNodeAttribute>().nodeType == nodeType);
        }

        public static Dictionary<RefactorNodeGroup, List<NodeEntryModel>> GetNodeEntries()
        {
            var nodes = new Dictionary<RefactorNodeGroup, List<NodeEntryModel>>();
            var nodeTypeGroups = typeof(RefactorNodeBase).FindAllDerivedTypes()
                .Where(x => x.HasAttribute<RefactorNodeAttribute>())
                .GroupBy(x => x.GetAttribute<RefactorNodeAttribute>().nodeGroup);

            foreach (var nodeTypeGroup in nodeTypeGroups.OrderBy(x => x.Key))
            {
                var nodeList = new List<NodeEntryModel>();
                nodes.Add(nodeTypeGroup.Key, nodeList);
                foreach (var nodeType in nodeTypeGroup.OrderBy(x => x.GetAttribute<RefactorNodeAttribute>().ToString()))
                {
                    if (nodeType == typeof(ReferenceNode))
                    {
                        continue;
                    }

                    var att = nodeType.GetAttribute<RefactorNodeAttribute>();
                    var nodeEntry = new NodeEntryModel
                    {
                        nodeName = att.nodeType.ToString(),
                        nodeType = att.nodeType,
                        nodeGroup = att.nodeGroup
                    };
                    nodeList.Add(nodeEntry);
                }
            }

            var flowCharts = NodeGraphManager.FlowCharts.Values;
            foreach (var flowChart in flowCharts.Where(x => x.IsReference))
            {
                nodes[RefactorNodeGroup.PartitionOperations].Add(new NodeEntryModel
                {
                    nodeName = flowChart.Name,
                    nodeType = RefactorNodeType.Reference,
                    nodeGroup = RefactorNodeGroup.PartitionOperations
                });
            }
            return nodes;
        }

        public static string GetCustomNodesPath()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dir = Path.Combine(dir ?? throw new InvalidOperationException(), "Resources");
            return dir;
        }
        #endregion
    }
}