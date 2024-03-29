﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using NodeGraph;
using NodeGraph.Commands;
using NodeGraph.Model;

namespace RefactorGraph.Nodes
{
    public abstract class RefactorNodeBase : Node
    {
        #region Fields
        public const string START_PORT_NAME = "Start";
        public const string DONE_PORT_NAME = "Done";
        public const string LOOP_PORT_NAME = "Loop";
        #endregion

        #region Properties
        protected virtual bool AllowEditingHeaderOverride => false;
        protected virtual bool HasInput => true;
        protected virtual bool HasDone => true;
        protected virtual bool HasLoop => false;
        #endregion

        #region Constructors
        protected RefactorNodeBase(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            HelpCommand = new RelayCommand(ShowHelp);
        }
        #endregion

        #region Methods
        public override void OnCreate()
        {
            var refactorNodeAtt = GetType().GetAttribute<RefactorNodeAttribute>();
            var nodeType = refactorNodeAtt.nodeType;
            var matches = Regex.Matches(nodeType.ToString(), "[A-Z][a-z]*");
            var header = matches.Cast<Match>().Aggregate(string.Empty, (current, match) => current + match.Value + " ");
            Header = header.Trim();
            HeaderFontColor = Brushes.White;
            AllowEditingHeader = AllowEditingHeaderOverride;
            AllowCircularConnection = true;
            HeaderBackgroundColor = NodeColors.brushes[refactorNodeAtt.nodeGroup];

            if (HasInput)
            {
                CreateInputFlowPort(START_PORT_NAME);
            }
            if (HasDone)
            {
                CreateOutputFlowPort(DONE_PORT_NAME);
            }
            if (HasLoop)
            {
                CreateOutputFlowPort(LOOP_PORT_NAME);
            }

            base.OnCreate();
        }

        protected void CreateInputFlowPort(string name)
        {
            NodeGraphManager.CreateNodeFlowPort(false, Guid.NewGuid(), this, true, name: name, displayName: name);
        }

        protected void CreateOutputFlowPort(string name)
        {
            NodeGraphManager.CreateNodeFlowPort(false, Guid.NewGuid(), this, false, name: name, displayName: name);
        }

        protected override void OnPostExecute(Connector connector)
        {
            if (HasDone)
            {
                ExecutionState = ExecutePort(DONE_PORT_NAME);
            }
            if (ExecutionState == ExecutionState.Executing)
            {
                ExecutionState = ExecutionState.Executed;
            }
            base.OnPostExecute(connector);
        }

        protected TValue GetPortValue<TValue>(string portName, TValue defaultValue = default)
        {
            var port = NodeGraphManager.FindNodePropertyPort(this, portName);
            if (port == null)
            {
                return defaultValue;
            }
            NodeGraphManager.FindConnectedPorts(port, out var connectedPorts);
            var otherPort = connectedPorts.Count > 0 ? connectedPorts[0] as NodePropertyPort : null;
            return otherPort != null ? (TValue)otherPort.Value : (TValue)port.Value;
        }

        protected ExecutionState ExecutePort(string portName)
        {
            var port = OutputFlowPorts.FirstOrDefault(p => p.Name == portName);
            if (port == null)
            {
                NodeGraphManager.AddScreenLog(Owner, $"Port {portName} not found");
                return ExecutionState.Failed;
            }
            var connector = port.Connectors.FirstOrDefault();
            return connector?.Execute() ?? ExecutionState.Executed;
        }

        private void ShowHelp()
        {
            var refactorNodeAtt = GetType().GetAttribute<RefactorNodeAttribute>();
            var nodeType = refactorNodeAtt.nodeType;
            Utils.FlowChartWindow.ShowHelp(nodeType.ToString());
        }
        #endregion
    }
}