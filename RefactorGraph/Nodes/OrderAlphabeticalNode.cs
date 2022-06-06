using System;
using System.Windows.Media;
using RefactorGraphdCore.Data;
using NodeGraph.Model;

namespace RefactorGraph
{
    [Node]
    [RefactorNode(group = RefactorNodeGroup.Processing, nodeType = RefactorNodeType.OrderAlphabetical)]
    [NodeFlowPort(INPUT_PORT_NAME, "", true)]
    [NodeFlowPort(OUTPUT_PORT_NAME, "", false)]
    public class OrderAlphabeticalNode : RefactorNodeBase
    {
        #region Fields
        public const string INPUT_PORT_NAME = "Input";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string COLLECTION_IN_PORT_NAME = "CollectionIn";
        public const string COLLECTION_OUT_PORT_NAME = "CollectionOut";
        public const string DESCENDING_PORT_NAME = "Descending";
        public const string IGNORE_WHITESPACE_PORT_NAME = "IgnoreWhitespace";

        [NodePropertyPort(COLLECTION_IN_PORT_NAME, true, typeof(ChunkCollection), null, false, DisplayName = "Collection")]
        public ChunkCollection CollectionIn;

        [NodePropertyPort(COLLECTION_OUT_PORT_NAME, false, typeof(ChunkCollection), null, false, DisplayName = "Collection")]
        public ChunkCollection CollectionOut;

        [NodePropertyPort(DESCENDING_PORT_NAME, true, typeof(bool), false, true)]
        public bool Descending;
        
        [NodePropertyPort(IGNORE_WHITESPACE_PORT_NAME, true, typeof(bool), true, true)]
        public bool IgnoreWhitespace;
        #endregion

        #region Constructors
        public OrderAlphabeticalNode(Guid guid, FlowChart flowChart) : base(guid, flowChart)
        {
            Header = "OrderAlphabetical";
            HeaderBackgroundColor = Brushes.DarkBlue;
            AllowEditingHeader = false;
        }
        #endregion

        #region Methods
        public override void OnExecute(Connector prevConnector)
        {
            base.OnExecute(prevConnector);

            var collection = GetPortValue<ChunkCollection>(COLLECTION_IN_PORT_NAME);
            if (collection != null)
            {
                var descending = GetPortValue(DESCENDING_PORT_NAME, Descending);
                var ignoreWhitespace = GetPortValue(IGNORE_WHITESPACE_PORT_NAME, IgnoreWhitespace);
                collection.Sort((a, b) =>
                {
                    var strA = ignoreWhitespace ? a.content.Trim() : a.content;
                    var strB = ignoreWhitespace ? b.content.Trim() : b.content;
                    var cmp = string.Compare(strA, strB, StringComparison.Ordinal);
                    return descending ? -cmp : cmp;
                });
                _success = true;
            }
            SetPortValue(COLLECTION_OUT_PORT_NAME, collection);
        }

        public override void OnPostExecute(Connector prevConnector)
        {
            base.OnPostExecute(prevConnector);
            ExecutePort(OUTPUT_PORT_NAME);
        }
        #endregion
    }
}