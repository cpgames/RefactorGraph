using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RefactorGraph
{
    internal class NodeListModel
    {
        #region Fields
        public readonly Dictionary<RefactorNodeGroup, List<NodeEntryModel>> nodes = new Dictionary<RefactorNodeGroup, List<NodeEntryModel>>();
        #endregion

        #region Properties
        public static NodeListModel Instance { get; } = new NodeListModel();
        #endregion

        #region Constructors
        public NodeListModel()
        {
            Load();
        }
        #endregion

        #region Methods
        public void Load()
        {
            nodes.Clear();

            var nodeTypeGroups = typeof(RefactorNodeBase).FindAllDerivedTypes()
                .Where(x => x.HasAttribute<RefactorNodeAttribute>())
                .GroupBy(x => x.GetAttribute<RefactorNodeAttribute>().group);

            foreach (var nodeTypeGroup in nodeTypeGroups.OrderBy(x => x.Key))
            {
                var nodeList = new List<NodeEntryModel>();
                nodes.Add(nodeTypeGroup.Key, nodeList);
                foreach (var nodeType in nodeTypeGroup.OrderBy(x => x.GetAttribute<RefactorNodeAttribute>()))
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

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var customNodes = Utils.GetGraphFiles(dir);
            foreach (var customNode in customNodes)
            {
                nodes[RefactorNodeGroup.Custom].Add(new NodeEntryModel
                {
                    nodeName = customNode,
                    nodeType = RefactorNodeType.Reference
                });
            }
        }
        #endregion
    }
}