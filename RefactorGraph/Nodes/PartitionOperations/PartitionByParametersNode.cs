﻿using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByParameters)]
    public class PartitionByParametersNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";
        public const string PARAMETER_FILTER_PORT_NAME = "ParameterFilter";
        public const string PARAMETER_PORT_NAME = "Parameter";

        private const string PARAMS_REGEX = "(?:\\b[\\w\\s.]+\\b|" + // words
            "(<(?:[^<>]++|(?-1))*>)|" + // <> brackets
            "(\\((?:[^()]++|(?-1))*\\))|" + // () brackets
            "(\"(?:[^\"\"]++|(?-1))*\")|" + // quotes
            "\\s*=>\\s*|" + // lambda
            "({(?:[^{}]++|(?-1))*}))+"; // {} brackets

        // Inputs
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(PARAMETER_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ParameterFilter;

        // Outputs
        [NodePropertyPort(PARAMETER_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition Parameter;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByParametersNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            Parameter = null;
            base.OnPreExecute(prevConnector);
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Skipped;
                return;
            }
            PartitionParameters(Partition);
        }

        private void PartitionParameters(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, PARAMS_REGEX);
            if (partitions != null)
            {
                foreach (var p in partitions)
                {
                    Parameter = p;
                    if (ApplyFilter())
                    {
                        ExecutionState = ExecutePort(LOOP_PORT_NAME);
                        if (ExecutionState == ExecutionState.Failed)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool ApplyFilter()
        {
            ParameterFilter = GetPortValue(PARAMETER_FILTER_PORT_NAME, ParameterFilter);
            if (!Partition.IsMatch(Parameter, ParameterFilter))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}