using System;
using System.Collections;
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

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(object), null, false, Serialized = false)]
        public object Source;
        #endregion

        #region Constructors
        public PrintNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);
            Source = GetPortValue<object>(SOURCE_PORT_NAME);
            if (Source != null)
            {
                string result;
                if (Source is IList list)
                {
                    result = string.Empty;
                    for (var i = 0; i < list.Count; i++)
                    {
                        result += list[i].ToString();
                        if (i < list.Count - 1)
                        {
                            result += ", ";
                        }
                    }
                }
                else
                {
                    result = Source.ToString();
                }
                NodeGraphManager.AddScreenLog(Owner, result);
            }
        }
        #endregion
    }
}