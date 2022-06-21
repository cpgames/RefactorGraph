using System;
using System.Collections;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.Other
{
    [Node]
    [RefactorNode(RefactorNodeGroup.Other, RefactorNodeType.ConvertToString)]
    public class ConvertToStringNode : RefactorNodeBase
    {
        #region Fields
        public const string SOURCE_PORT_NAME = "Source";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(object), null, false)]
        public object Source;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Constructors
        public ConvertToStringNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<object>(SOURCE_PORT_NAME);
            if (Source != null)
            {
                if (Source is IList list)
                {
                    Result = string.Empty;
                    for (var i = 0; i < list.Count; i++)
                    {
                        Result += list[i].ToString();
                        if (i < list.Count - 1)
                        {
                            Result += ", ";
                        }
                    }
                }
                else
                {
                    Result = Source.ToString();
                }
            }
            SetPortValue(RESULT_PORT_NAME, Result);
            _success = true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}