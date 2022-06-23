using System;
using System.Collections;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.StringFormat)]
    public class StringFormatNode : RefactorNodeBase
    {
        #region Fields
        public const string FORMAT_PORT_NAME = "Format";
        public const string ARGS_PORT_NAME = "Args";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(FORMAT_PORT_NAME, true, typeof(string), "", true)]
        public string Format;

        [NodePropertyPort(ARGS_PORT_NAME, true, typeof(object), null, false)]
        public object Args;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Properties
        public override bool Success => Result != null;
        #endregion

        #region Constructors
        public StringFormatNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Result = null;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Format = GetPortValue(FORMAT_PORT_NAME, Format);
            Args = GetPortValue<object>(ARGS_PORT_NAME);
            if (Args != null)
            {
                try
                {
                    if (Args is IList list)
                    {
                        var strArray = new object[list.Count];
                        for (var i = 0; i < list.Count; i++)
                        {
                            strArray[i] = list[i].ToString();
                        }
                        Result = string.Format(Format, strArray);
                    }
                    else
                    {
                        Result = string.Format(Format, Args);
                    }
                    SetPortValue(RESULT_PORT_NAME, Result);
                }
                catch (Exception e)
                {
                    NodeGraphManager.AddScreenLog(Owner, e.Message);
                }
            }
        }
        #endregion
    }
}