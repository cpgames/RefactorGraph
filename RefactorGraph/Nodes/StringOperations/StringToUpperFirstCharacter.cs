using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.StringToUpperFirstCharacter)]
    public class StringToUpperFirstCharacter : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(string), "", true)]
        public string Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Properties
        public override bool Success => Result != null;
        #endregion

        #region Constructors
        public StringToUpperFirstCharacter(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Result = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue(SOURCE_PORT_NAME, Source);
            if (!string.IsNullOrEmpty(Source))
            {
                if (Source.Length == 1)
                {
                    Result = Source.ToUpper();
                }
                else
                {
                    var first = Source.Substring(0, 1);
                    var second = Source.Substring(1);
                    first = first.ToUpper();
                    Result = first + second;
                }
            }
        }
        #endregion
    }
}