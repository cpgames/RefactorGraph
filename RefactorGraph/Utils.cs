using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using EnvDTE;
using MdXaml;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using NodeGraph;
using NodeGraph.Model;
using PCRE;
using RefactorGraph.Nodes;
using RefactorGraph.Nodes.Other;
using RefactorGraph.Nodes.PartitionOperations;

namespace RefactorGraph
{
    public static class Utils
    {
        #region Fields
        private static DesignerWindowControl _flowChartWindow;
        public static Action refreshAction;
        public static Action flowChartChanged;
        #endregion

        #region Properties
        public static DesignerWindowControl FlowChartWindow
        {
            get => _flowChartWindow;
            set
            {
                _flowChartWindow = value;
                flowChartChanged?.Invoke();
            }
        }
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
                var filePath = CreateGraphFilePath(graphName);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("FlowGraph file not found.", filePath);
                }

                File.Delete(filePath);

                var flowChart = NodeGraphManager.FlowCharts.Values.FirstOrDefault(x => x.Name == graphName);
                if (flowChart != null)
                {
                    NodeGraphManager.DestroyFlowChart(flowChart.Guid);
                }
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

            var startNodes = NodeGraphManager.FindNode(flowChart, "Start");
            if (startNodes.Count == 0)
            {
                NodeGraphManager.AddScreenLog(flowChart, "You need to place a 'Start' node.");
                return false;
            }
            if (startNodes.Count > 1)
            {
                NodeGraphManager.AddScreenLog(flowChart, "Only single 'Start' must exist.");
                return false;
            }

            startNode = startNodes[0] as StartNode;
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

        public static void Refactor(FlowChart flowChart)
        {
            if (flowChart == null)
            {
                return;
            }
            NodeGraphManager.ClearScreenLogs(flowChart);
            if (!ValidateGraph(flowChart, out var startNode))
            {
                return;
            }
            try
            {
                startNode.Execute(null);
            }
            catch (Exception e)
            {
                NodeGraphManager.AddScreenLog(flowChart, e.Message);
            }
        }

        public static Partition GetDocumentPartition(TextDocument document)
        {
            var documentPartition = new Partition();
            var editPoint = document.StartPoint.CreateEditPoint();
            documentPartition.data = editPoint.GetText(document.EndPoint);
            return documentPartition;
        }

        public static bool SetDocumentPartition(TextDocument document, Partition partition)
        {
            var editPoint = document.StartPoint.CreateEditPoint();
            var originalText = editPoint.GetText(document.EndPoint);
            if (originalText != partition.data)
            {
                document.Selection.SelectAll();
                document.Selection.Insert(partition.data);
                return true;
            }
            return false;
        }

        public static Project GetActiveProject()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            if (!(dte.ActiveSolutionProjects is Array projects) || projects.Length == 0)
            {
                return null;
            }
            return projects.GetValue(0) as Project;
        }

        public static TextDocument GetActiveDocument()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            if (!(dte.ActiveDocument?.Object() is TextDocument textDocument))
            {
                return null;
            }
            return textDocument;
        }

        public static List<ProjectItem> GetProjectItemsInSolution(string projectFilter = "", string filenameFilter = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            var projectItems = new List<ProjectItem>();
            if (dte.Solution == null)
            {
                return projectItems;
            }
            var projects = dte.Solution.Projects.GetEnumerator();
            while (projects.MoveNext())
            {
                var project = projects.Current as Project;
                if (project == null || !StringMatchesRegex(project.Name, projectFilter))
                {
                    continue;
                }
                var items = project.ProjectItems.GetEnumerator();
                while (items.MoveNext())
                {
                    var currentItem = (ProjectItem)items.Current;
                    var childItem = GetProjectItemsInProjectItem(currentItem, projectItems);
                    AppendProjectItem(childItem, projectItems, filenameFilter);
                }
            }
            return projectItems;
        }

        public static List<ProjectItem> GetProjectItemsInProject(Project project, string filenameFilter = "")
        {
            var projectItems = new List<ProjectItem>();
            var items = project.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                var childItem = GetProjectItemsInProjectItem(currentItem, projectItems);
                AppendProjectItem(childItem, projectItems, filenameFilter);
            }
            return projectItems;
        }

        public static ProjectItem GetProjectItemsInProjectItem(ProjectItem item, List<ProjectItem> projectItems, string filenameFilter = "")
        {
            if (item?.ProjectItems == null)
            {
                return item;
            }

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                var childItem = GetProjectItemsInProjectItem(currentItem, projectItems, filenameFilter);
                AppendProjectItem(childItem, projectItems, filenameFilter);
            }
            return item;
        }

        public static List<Project> GetProjectsInSolution(string projectFilter = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            var projects = dte.Solution?.Projects
                .OfType<Project>()
                .Where(project => StringMatchesRegex(project.Name, projectFilter)).ToList();
            return projects;
        }

        private static void AppendProjectItem(ProjectItem item, ICollection<ProjectItem> projectItems, string filenameFilter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (item != null &&
                item.Kind == Constants.vsProjectItemKindPhysicalFile &&
                StringMatchesRegex(item.Name, filenameFilter))
            {
                projectItems.Add(item);
            }
        }

        public static ProjectItem GetFiles(ProjectItem item, List<ProjectItem> projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (item.ProjectItems == null)
            {
                return item;
            }

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                projectItems.Add(GetFiles(currentItem, projectItems));
            }

            return item;
        }

        public static bool StringMatchesRegex(string str, string pattern)
        {
            return
                string.IsNullOrEmpty(pattern) ||
                PcreRegex.IsMatch(str, pattern);
        }

        public static void SetMdStyle()
        {
            var paragraph = MarkdownStyle.GithubLike.Resources.Values
                .OfType<Style>()
                .FirstOrDefault(x => x.TargetType == typeof(Paragraph));
            if (paragraph == null)
            {
                return;
            }
            var triggers = new[] { "Heading1", "Heading2", "Heading3", "Heading4" };
            foreach (var triggerName in triggers)
            {
                var t = paragraph.Triggers
                    .OfType<Trigger>()
                    .FirstOrDefault(x => x.Value.ToString() == triggerName);
                if (t == null)
                {
                    continue;
                }
                var s = t.Setters
                    .OfType<Setter>()
                    .FirstOrDefault(x => x.Property.Name == "Foreground");
                if (s == null)
                {
                    continue;
                }
                s.Value = new SolidColorBrush(Colors.White);
            }
        }

        public static ScrollViewer FindScrollViewer(this FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }
            // Border is the first child of first child of a ScrolldocumentViewer
            var firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            var border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;
            return border?.Child as ScrollViewer;
        }
        #endregion
    }
}