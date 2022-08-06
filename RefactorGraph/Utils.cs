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
using Process = System.Diagnostics.Process;

namespace RefactorGraph
{
    public static class Utils
    {
        #region Fields
        private static readonly List<string> _includeFolderPaths = new List<string>();
        private static readonly Dictionary<Guid, string> _guidToPathMap = new Dictionary<Guid, string>();

        private static DesignerWindowControl _flowChartWindow;
        public static Action refreshed;
        public static Action flowChartChanged;
        public static Action beginRefactor;
        public static Action endRefactor;
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
        public static IEnumerable<string> GetIncludedFolderPaths()
        {
            return _includeFolderPaths;
        }

        public static bool AddIncludeFolder(string folderPath)
        {
            var absFolderPath = Path.GetFullPath(folderPath);
            if (!Directory.Exists(absFolderPath))
            {
                return false;
            }
            if (_includeFolderPaths.Contains(absFolderPath))
            {
                return false;
            }
            _includeFolderPaths.Add(absFolderPath);
            return true;
        }

        public static bool RemoveIncludeFolder(string folderPath)
        {
            if (!_includeFolderPaths.Contains(folderPath))
            {
                return false;
            }
            _includeFolderPaths.Remove(folderPath);
            return true;
        }

        public static bool GetGraphPath(Guid guid, out string path)
        {
            return _guidToPathMap.TryGetValue(guid, out path);
        }

        public static void RemoveFlowChart(Guid guid)
        {
            if (_guidToPathMap.ContainsKey(guid))
            {
                _guidToPathMap.Remove(guid);
                NodeGraphManager.DestroyFlowChart(guid);
            }
        }

        public static string GetOrCreateDefaultDir()
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

        public static IEnumerable<string> GetGraphFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.rgraph");
        }

        public static string CreateGraphFilePath(string graphName, string folderPath)
        {
            graphName = $"{graphName}.rgraph";
            return Path.Combine(folderPath, graphName);
        }

        public static bool RenameFile(Guid graphGuid, string newGraphName)
        {
            try
            {
                var oldFilePath = _guidToPathMap[graphGuid];
                var newFilePath = CreateGraphFilePath(newGraphName, Path.GetDirectoryName(oldFilePath));
                if (_guidToPathMap.Values.Contains(newFilePath))
                {
                    throw new Exception(@"File {newFilePath} already exists.");
                }
                _guidToPathMap[graphGuid] = newFilePath;
                File.Move(oldFilePath, newFilePath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Rename failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static bool Save(FlowChart flowChart, string filePath)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineChars = "\n",
                    NewLineHandling = NewLineHandling.Replace,
                    NewLineOnAttributes = false
                };
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
                _guidToPathMap[flowChart.Guid] = filePath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static void Load(string filePath, out FlowChart flowChart)
        {
            try
            {
                flowChart = null;
                var kvp = _guidToPathMap.FirstOrDefault(x => x.Value == filePath);
                if (kvp.Key != default)
                {
                    flowChart = NodeGraphManager.FindFlowChart(kvp.Key);
                    return;
                }
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File <{filePath}> not found.", filePath);
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
                                if (_guidToPathMap.ContainsKey(guid))
                                {
                                    throw new Exception("FlowChart with this guid already exists.");
                                }
                                var type = Type.GetType(reader.GetAttribute("Type") ?? throw new InvalidOperationException());

                                flowChart = NodeGraphManager.FindFlowChart(guid);
                                if (flowChart == null)
                                {
                                    flowChart = NodeGraphManager.CreateFlowChart(true, guid, type);
                                    flowChart.ReadXml(reader);
                                    flowChart.OnDeserialize();
                                }
                                break;
                            }
                        }
                    }
                }
                if (flowChart == null)
                {
                    throw new Exception("FlowChart failed to parse XML.");
                }
                _guidToPathMap[flowChart.Guid] = filePath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static bool Delete(Guid guid)
        {
            try
            {
                if (!_guidToPathMap.TryGetValue(guid, out var filePath))
                {
                    return false;
                }
                _guidToPathMap.Remove(guid);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("FlowGraph file not found.", filePath);
                }
                File.Delete(filePath);
                NodeGraphManager.DestroyFlowChart(guid);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static void OpenInExplorer(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
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
                if (beginRefactor != null)
                {
                    beginRefactor();
                }
                startNode.Execute(null);
                endRefactor?.Invoke();
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