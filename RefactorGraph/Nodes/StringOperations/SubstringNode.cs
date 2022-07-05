using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.Substring)]
    public class SubstringNode : RefactorNodeBase
    {
        #region Fields
        public const string INDEX_PORT_NAME = "Index";
        public const string LENGTH_PORT_NAME = "Length";
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(string), "", true)]
        public string Source;

        [NodePropertyPort(INDEX_PORT_NAME, true, typeof(int), 0, true)]
        public int Index;

        [NodePropertyPort(LENGTH_PORT_NAME, true, typeof(int), 0, true)]
        public int Length;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Properties
        public override bool Success => Result != null;
        #endregion

        #region Constructors
        public SubstringNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
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

            Source = GetPortValue(SOURCE_PORT_NAME, SOURCE_PORT_NAME);
            Index = GetPortValue(INDEX_PORT_NAME, Index);
            Length = GetPortValue(LENGTH_PORT_NAME, Length);

            if (!string.IsNullOrEmpty(Source) &&
                Index >= 0 &&
                Index + Length < Source.Length)
            {
                Result = Length > 0 ?
                    Source.Substring(Index, Length) :
                    Source.Substring(Index);
            }
        }
        #endregion
    }
}