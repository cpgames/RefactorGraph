using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph
{
    public static class Utils
    {
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

        public static bool Save(string graphName, FlowChart flowChart)
        {
            try
            {
                var filePath = CreateGraphFilePath(graphName);
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

        public static void Load(string graphName, out FlowChart flowChart)
        {
            var filePath = CreateGraphFilePath(graphName);
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

                            flowChart = NodeGraphManager.CreateFlowChart(true, guid, type);
                            flowChart.ReadXml(reader);
                            flowChart.OnDeserialize();
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
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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
        #endregion
    }
}