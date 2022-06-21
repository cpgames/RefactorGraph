using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.Start)]
    public class StartNode : RefactorNodeBase
    {
        #region Fields
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Result;
        #endregion

        #region Properties
        protected override bool HasInput => false;
        #endregion

        #region Constructors
        public StartNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            if (Result != null)
            {
                SetPortValue(RESULT_PORT_NAME, Result);
                _success = Result != null;
            }
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(OUTPUT_PORT_NAME);
            if (_success)
            {
                Result.Rasterize();
            }
        }
        #endregion
    }
}