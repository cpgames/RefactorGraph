using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByProperty)]
    public class PartitionByPropertyNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string MODIFIER_FILTER_PORT_NAME = "ModifierFilter";
        public const string PROPERTY_TYPE_FILTER_PORT_NAME = "PropertyTypeFilter";
        public const string PROPERTY_NAME_FILTER_PORT_NAME = "PropertyNameFilter";

        public const string PROPERTY_PORT_NAME = "Property";
        public const string SCOPE_PORT_NAME = "Scope";
        public const string MODIFIER_PORT_NAME = "Modifier";
        public const string PROPERTY_TYPE_PORT_NAME = "PropertyType";
        public const string PROPERTY_NAME_PORT_NAME = "PropertyName";
        public const string PROPERTY_BODY_PORT_NAME = "PropertyBody";

        private const string PROPERTY_REGEX = @"(?:namespace|new)\s*(*SKIP)(*F)|" + // skip namespace
            @"(?:public\\s*|private\\s*|protected\\s*|internal\\s*)?" + // scope
            @"(?:abstract\s*|static\s*|override\s*)?" + // modifier
            @"(?:(?:interface\b|class\b|struct\b|enum\b)(*SKIP)(*F))?" + // skip interface, class, struct, enum
            @"(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?[?]?)\s+" + // property type
            @"(?:\b[\w.]+\b\s*)" + // property name
            @"(?:({(?:[^{}]++|(?-1))*})|=>[\s\w]*;)"; // property body
        private const string DEF_REGEX = @"[\s\S]+?(?=\s*[{=])";
        private const string SCOPE_REGEX = @"(?:public|private|protected|internal)";
        private const string MODIFIER_REGEX = @"(?:abstract|static|override)";
        private const string RETURN_TYPE_REGEX = @"\A\s*\K(?:\b[\w.]+\b(<(?:[^<>]++|(?-1))*>)?[?]?)(?!\s*\()";
        private const string NAME_REGEX = @"(?:\b[\w.]+\b\s*)(<(?:[^<>]++|(?-1))*>)?";
        private const string BODY_BLOCK_REGEX = @"({(?:[^{}]++|(?-1))*})|=>\s*.*(?=;\Z)";
        private const string BODY_REGEX = @"{\s*\K[\S\s]*[^\s](?=\s*})|=>\s*\K.*(?=\s*\Z)";

        private static readonly string[] DEF_BODY = { DEF_REGEX, BODY_BLOCK_REGEX };
        private static readonly string[] SCOPE_MODIFIER_RETURN_TYPE_NAME_PARAMS = { SCOPE_REGEX, MODIFIER_REGEX, RETURN_TYPE_REGEX, NAME_REGEX };

        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;
        
        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Protected | RefactorGraph.Scope.Private | RefactorGraph.Scope.Internal | RefactorGraph.Scope.Public | RefactorGraph.Scope.Scopeless, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(MODIFIER_FILTER_PORT_NAME, true, typeof(PropertyModifier), PropertyModifier.Static | PropertyModifier.Abstract | PropertyModifier.Virtual | PropertyModifier.None, true)]
        public PropertyModifier ModifierFilter;

        [NodePropertyPort(PROPERTY_TYPE_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string PropertyTypeFilter;

        [NodePropertyPort(PROPERTY_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string PropertyNameFilter;
        
        [NodePropertyPort(PROPERTY_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Property;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Scope;

        [NodePropertyPort(MODIFIER_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Modifier;

        [NodePropertyPort(PROPERTY_TYPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PropertyType;

        [NodePropertyPort(PROPERTY_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PropertyName;
        
        [NodePropertyPort(PROPERTY_BODY_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition PropertyBody;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByPropertyNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            Property = null;
            Scope = null;
            Modifier = null;
            PropertyType = null;
            PropertyName = null;
            PropertyBody = null;
        }

        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PartitionProperties(Partition);
        }

        private void PartitionProperties(Partition partition)
        {
            var partitions = Partition.PartitionByRegexMatch(partition, PROPERTY_REGEX);
            foreach (var p in partitions)
            {
                if (ExecutionState == ExecutionState.Failed)
                {
                    return;
                }
                var def_body = Partition.PartitionByRegexMatch(p, DEF_BODY);
                var scope_modifier_returnType_name_params = Partition.PartitionByRegexMatch(def_body[0], SCOPE_MODIFIER_RETURN_TYPE_NAME_PARAMS);
                Property = p;
                Scope = scope_modifier_returnType_name_params[0];
                Modifier = scope_modifier_returnType_name_params[1];
                PropertyType = scope_modifier_returnType_name_params[2];
                PropertyName = scope_modifier_returnType_name_params[3];
                PropertyBody = Partition.PartitionByFirstRegexMatch(def_body[1], BODY_REGEX);
                var executionState = ExecutionState.Executing;
                if (ApplyFilter())
                {
                    executionState = ExecutePort(LOOP_PORT_NAME);
                    if (executionState == ExecutionState.Failed)
                    {
                        ExecutionState = ExecutionState.Failed;
                        return;
                    }
                }
                if (executionState == ExecutionState.Skipped)
                {
                    break;
                }
            }
        }

        private bool ApplyFilter()
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            var scopeEnum = RefactorGraph.Scope.Scopeless;
            if (Scope != null)
            {
                switch (Scope.data)
                {
                    case "public":
                        scopeEnum = RefactorGraph.Scope.Public;
                        break;
                    case "private":
                        scopeEnum = RefactorGraph.Scope.Private;
                        break;
                    case "internal":
                        scopeEnum = RefactorGraph.Scope.Internal;
                        break;
                    case "protected":
                        scopeEnum = RefactorGraph.Scope.Protected;
                        break;
                }
            }
            if ((scopeEnum & ScopeFilter) == 0)
            {
                return false;
            }

            ModifierFilter = GetPortValue(MODIFIER_FILTER_PORT_NAME, ModifierFilter);
            var modifierEnum = PropertyModifier.None;
            if (Modifier != null)
            {
                switch (Modifier.data)
                {
                    case "static":
                        modifierEnum = PropertyModifier.Static;
                        break;
                    case "virtual":
                        modifierEnum = PropertyModifier.Virtual;
                        break;
                    case "abstract":
                        modifierEnum = PropertyModifier.Abstract;
                        break;
                }
            }
            if ((modifierEnum & ModifierFilter) == 0)
            {
                return false;
            }

            PropertyTypeFilter = GetPortValue(PROPERTY_TYPE_FILTER_PORT_NAME, PropertyTypeFilter);
            if (!Partition.IsMatch(PropertyType, PropertyTypeFilter))
            {
                return false;
            }

            PropertyNameFilter = GetPortValue(PROPERTY_NAME_FILTER_PORT_NAME, PropertyNameFilter);
            if (!Partition.IsMatch(PropertyName, PropertyNameFilter))
            {
                return false;
            }
            
            return true;
        }
        #endregion
    }
}