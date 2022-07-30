using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeGraph;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.StringOperations
{
    [Node(ViewModelType = typeof(DynamicNodeViewModel))]
    [RefactorNode(RefactorNodeGroup.StringOperations, RefactorNodeType.StringFormat)]
    public class StringFormatNode : DynamicNodeBase
    {
        #region Fields
        public const string FORMAT_PORT_NAME = "Format";
        public const string RESULT_PORT_NAME = "Result";

        [NodePropertyPort(FORMAT_PORT_NAME, true, typeof(string), "", true)]
        public string Format;

        [NodePropertyPort(RESULT_PORT_NAME, false, typeof(string), "", false)]
        public string Result;
        #endregion

        #region Properties
        protected override IList DynamicPorts => InputPropertyPorts;
        protected override int MinDynamicPorts => 1;
        #endregion

        #region Constructors
        public StringFormatNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            base.OnCreate();

            AddDynamicPort(null, null);
        }

        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Result = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Format = GetPortValue(FORMAT_PORT_NAME, Format);
            var argPorts = InputPropertyPorts
                .Where(x => x.Name.Contains("Args"));
            var args = new List<object>();
            foreach (var port in argPorts)
            {
                NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
                var otherPort = connectedPorts.Count > 0 ? connectedPorts[0] as NodePropertyPort : null;
                if (otherPort != null)
                {
                    var value = otherPort.Value ?? string.Empty;
                    if (value is IList list)
                    {
                        foreach (var listValue in list)
                        {
                            args.Add(listValue.ToString());
                        }
                    }
                    else
                    {
                        args.Add(value.ToString());
                    }
                }
            }
            var strArgs = args.ToArray();
            try
            {
                Result = string.Format(Format, strArgs);
            }
            catch (Exception e)
            {
                NodeGraphManager.AddScreenLog(Owner, e.Message);
                ExecutionState = ExecutionState.Failed;
            }
        }

        protected override void CreateDynamicPort(Guid guid)
        {
            var name = $"Args {DynamicPorts.Count - 1}";
            NodeGraphManager.CreateNodePropertyPort(false, guid, this, true, typeof(object), 
                null, name, false, displayName: name, serializeValue: false);
        }
        #endregion
    }
}