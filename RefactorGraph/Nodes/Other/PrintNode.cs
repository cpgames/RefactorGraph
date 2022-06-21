using System;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Print)]
    public class PrintNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(string), "", false)]
        public string Source;
        #endregion
        
        #region Constructors
        public PrintNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue(SOURCE_PORT_NAME, Source);
            NodeGraphManager.AddScreenLog(Owner, Source);
            _success = true;
        }
        #endregion
    }
}