using System;
using System.Collections.Generic;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.SortPartitions)]
    public class SortPartitionsNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_COLLECTION_PORT_NAME = "PartitionCollection";
        public const string SORTING_MAP_PORT_NAME = "SortingMap";
        public const string DESCENDING_PORT_NAME = "Descending";

        [NodePropertyPort(PARTITION_COLLECTION_PORT_NAME, true, typeof(List<Partition>), null, false, Serialized = false)]
        public List<Partition> PartitionCollection;

        [NodePropertyPort(SORTING_MAP_PORT_NAME, true, typeof(Dictionary<Partition, Partition>), null, false, Serialized = false)]
        public Dictionary<Partition, Partition> SortingMap;

        [NodePropertyPort(DESCENDING_PORT_NAME, true, typeof(bool), false, true)]
        public bool Descending;
        #endregion

        #region Constructors
        public SortPartitionsNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            PartitionCollection = GetPortValue<List<Partition>>(PARTITION_COLLECTION_PORT_NAME);
            var collectionOld = new List<Partition>(PartitionCollection);
            SortingMap = GetPortValue<Dictionary<Partition, Partition>>(SORTING_MAP_PORT_NAME);
            Descending = GetPortValue(DESCENDING_PORT_NAME, Descending);
            if (PartitionCollection == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            if (SortingMap == null)
            {
                PartitionCollection.Sort((a, b) => Descending ?
                    string.Compare(b.data, a.data, StringComparison.Ordinal) :
                    string.Compare(a.data, b.data, StringComparison.Ordinal));
            }
            else
            {
                PartitionCollection.Sort((a, b) =>
                {
                    if (SortingMap.ContainsKey(a) && SortingMap.ContainsKey(b))
                    {
                        return Descending ?
                            string.Compare(SortingMap[b].data, SortingMap[a].data, StringComparison.Ordinal) :
                            string.Compare(SortingMap[a].data, SortingMap[b].data, StringComparison.Ordinal);
                    }
                    return SortingMap.ContainsKey(a) ? -1 : 1;
                });
            }
            for (var i = 0; i < PartitionCollection.Count; i++)
            {
                var partition = PartitionCollection[i];
                var partitionOld = collectionOld[i];
                if (partition == partitionOld)
                {
                    continue;
                }
                Partition.Swap(partition, partitionOld);
                collectionOld[collectionOld.IndexOf(partition)] = partitionOld;
                collectionOld[i] = partition;
            }
        }
        #endregion
    }
}