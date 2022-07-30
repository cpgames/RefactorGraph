using System;
using System.IO;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.PartitionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.GetFilePartition)]
    public class GetFilePartitionNode : RefactorNodeBase
    {
        #region Fields
        public const string FILENAME_PORT_NAME = "FileName";
        public const string PARTITION_PORT_NAME = "Partition";

        [NodePropertyPort(FILENAME_PORT_NAME, true, typeof(string), null, true)]
        public string FileName;

        [NodePropertyPort(PARTITION_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;
        #endregion

        #region Constructors
        public GetFilePartitionNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            FileName = GetPortValue(FILENAME_PORT_NAME, FileName);
            if (string.IsNullOrEmpty(FileName))
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            var document = string.Empty;
            try
            {
                var sr = new StreamReader(FileName);
                document = sr.ReadToEnd();
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                NodeGraphManager.AddScreenLog(Utils.FlowChartWindow.FlowChartViewModel.Model, e.Message);
                ExecutionState = ExecutionState.Failed;
            }
            Partition = new Partition
            {
                data = document
            };
        }
        #endregion
    }
}