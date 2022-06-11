using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using gma.System.Windows;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    public static class Utils
    {
        #region Fields
        public static UserActivityHook actHook = new UserActivityHook(true, false);
        #endregion

        #region Methods
        public static string GetOrCreateDir(string dir = null)
        {
            if (dir == null)
            {
                dir = Directory.GetCurrentDirectory();
            }
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

        public static IEnumerable<string> GetGraphFiles(string dir = null)
        {
            dir = GetOrCreateDir(dir);
            var files = Directory.GetFiles(dir, "*.rgraph");
            return files.Select(Path.GetFileNameWithoutExtension);
        }

        public static string CreateGraphFilePath(string graphName, string dir = null)
        {
            graphName = $"{graphName}.rgraph";
            dir = GetOrCreateDir(dir);
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

        public static bool Save(string graphName, FlowChart flowChart, string dir = null)
        {
            try
            {
                var filePath = CreateGraphFilePath(graphName, dir);
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
                MessageBox.Show(e.Message, "Rename failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static void Load(string graphName, out FlowChart flowChart, string dir = null)
        {
            var filePath = CreateGraphFilePath(graphName, dir);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("FlowGraph file not found.", filePath);
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

        public static bool Delete(string graphName, string dir = null)
        {
            try
            {
                var filePath = CreateGraphFilePath(graphName, dir);
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

        public static bool ValidateGraph(FlowChart flowChart, out GetDocumentNode getDocumentNode, out SetDocumentNode setDocumentNode)
        {
            getDocumentNode = null;
            setDocumentNode = null;

            var getDocumentNodes = NodeGraphManager.FindNode(flowChart, "Get Document");
            if (getDocumentNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(flowChart, "You need to place a 'Get Document' node.");
                return false;
            }
            if (getDocumentNodes.Count > 1)
            {
                NodeGraphManager.AddScreenLog(flowChart, "Only single 'Get Document' must exist.");
                return false;
            }
            var setDocumentNodes = NodeGraphManager.FindNode(flowChart, "Set Document");
            if (setDocumentNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(flowChart, "You need to place a 'Set Document' node.");
                return false;
            }
            if (setDocumentNodes.Count > 1)
            {
                NodeGraphManager.AddScreenLog(flowChart, "Only single 'Set Document' must exist.");
                return false;
            }

            getDocumentNode = getDocumentNodes[0] as GetDocumentNode;
            setDocumentNode = setDocumentNodes[0] as SetDocumentNode;
            return false;
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
                .GroupBy(x => x.GetAttribute<RefactorNodeAttribute>().group);

            foreach (var nodeTypeGroup in nodeTypeGroups.OrderBy(x => x.Key))
            {
                var nodeList = new List<NodeEntryModel>();
                nodes.Add(nodeTypeGroup.Key, nodeList);
                foreach (var nodeType in nodeTypeGroup.OrderBy(x => x.GetAttribute<RefactorNodeAttribute>().ToString()))
                {
                    if (nodeType == typeof(ReferenceNode))
                    {
                        break;
                    }

                    var att = nodeType.GetAttribute<RefactorNodeAttribute>();
                    var nodeEntry = new NodeEntryModel
                    {
                        nodeName = att.nodeType.ToString(),
                        nodeType = att.nodeType
                    };
                    nodeList.Add(nodeEntry);
                }
            }

            var dir = GetCustomNodesPath();
            var customNodes = GetGraphFiles(dir);
            foreach (var customNode in customNodes)
            {
                nodes[RefactorNodeGroup.Custom].Add(new NodeEntryModel
                {
                    nodeName = customNode,
                    nodeType = RefactorNodeType.Reference
                });
            }
            return nodes;
        }

        public static string GetCustomNodesPath()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dir = Path.Combine(dir, "Resources");
            return dir;
        }
        #endregion
    }
}