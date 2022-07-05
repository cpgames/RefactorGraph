using System;
using NodeGraph.Model;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByClass)]
    public class PartitionByClassNode : RefactorNodeBase
    {
        #region Fields
        public const string PARTITION_PORT_NAME = "Partition";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string MODIFIER_FILTER_PORT_NAME = "ClassModifierFilter";
        public const string CATEGORY_FILTER_PORT_NAME = "TypeCategoryFilter";
        public const string ClASS_NAME_FILTER_PORT_NAME = "ClassNameFilter";

        public const string CLASS_PORT_NAME = "Class";
        public const string SCOPE_PORT_NAME = "Scope";
        public const string MODIFIER_PORT_NAME = "ClassModifier";
        public const string CATEGORY_PORT_NAME = "TypeCategory";
        public const string ClASS_NAME_PORT_NAME = "ClassName";
        public const string CLASS_BODY_PORT_NAME = "ClassBody";

        private const string CLASS_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)?" + // scope
            @"(?:abstract\s*|static\s*|sealed\s*)?" + // modifiers
            @"(?:class\s*|interface\s*|struct\s*|enum\s*)" + // category
            @"(?:\b[\w.]+\b\s*)" + // class name
            @"(?:[\s\w:,.:]*|(<(?:[^<>]++|(?-1))*>)*)*\s*" + // details
            @"({(?:[^{}]++|(?-1))*})"; // body
        private const string CLASS_DEF_REGEX = @"[\s\S]+?(?=\s*{)";
        private const string SCOPE_REGEX = @"(?:public|private|protected|internal)";
        private const string MODIFIER_REGEX = @"(?:abstract|static|sealed)";
        private const string CATEGORY_REGEX = @"(?:class|interface|struct|enum)";
        private const string NAME_REGEX = @"(?:\b[\w.]+\b\s*)(<(?:[^<>]++|(?-1))*>)?";
        private const string BODY_BLOCK_REGEX = @"({(?:[^{}]++|(?-1))*})";
        private const string BODY_REGEX = @"(?<={)\s*\K\S[\S\s]+(?<!\s)(?=\s*})";

        private static readonly string[] DEF_BODY = { CLASS_DEF_REGEX, BODY_BLOCK_REGEX };
        private static readonly string[] SCOPE_MODIFIER_CATEGORY_NAME = { SCOPE_REGEX, MODIFIER_REGEX, CATEGORY_REGEX, NAME_REGEX };
        // Inputs
        [NodePropertyPort(PARTITION_PORT_NAME, true, typeof(Partition), null, false, Serialized = false)]
        public Partition Partition;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Protected | RefactorGraph.Scope.Private | RefactorGraph.Scope.Internal | RefactorGraph.Scope.Public, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(MODIFIER_FILTER_PORT_NAME, true, typeof(ClassModifier), ClassModifier.Static | ClassModifier.Abstract | ClassModifier.None | ClassModifier.Sealed, true)]
        public ClassModifier ClassModifierFilter;

        [NodePropertyPort(CATEGORY_FILTER_PORT_NAME, true, typeof(TypeCategory), RefactorGraph.TypeCategory.Class | RefactorGraph.TypeCategory.Interface | RefactorGraph.TypeCategory.Struct | RefactorGraph.TypeCategory.Enum, true)]
        public TypeCategory TypeCategoryFilter;

        [NodePropertyPort(ClASS_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ClassNameFilter;

        // Outputs
        [NodePropertyPort(CLASS_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Class;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Scope;

        [NodePropertyPort(MODIFIER_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition Modifier;

        [NodePropertyPort(CATEGORY_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition TypeCategory;

        [NodePropertyPort(ClASS_NAME_PORT_NAME, false, typeof(Partition), null, false, Serialized = false)]
        public Partition ClassName;

        [NodePropertyPort(CLASS_BODY_PORT_NAME, false, typeof(Partition), null, true, Serialized = false)]
        public Partition ClassBody;
        #endregion

        #region Properties
        protected override bool HasLoop => true;
        #endregion

        #region Constructors
        public PartitionByClassNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        protected override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Partition = GetPortValue<Partition>(PARTITION_PORT_NAME);
            if (Partition == null)
            {
                ExecutionState = ExecutionState.Failed;
                return;
            }
            PartitionClasses(Partition);
        }

        private void PartitionClasses(Partition partition)
        {
            var partitions = partition.PartitionByRegexMatch(CLASS_REGEX);
            foreach (var p in partitions)
            {
                if (ExecutionState == ExecutionState.Failed)
                {
                    return;
                }
                PartitionClass(p);
            }
        }

        private void PartitionClass(Partition partition)
        {
            var def_body = partition.PartitionByRegexMatch(DEF_BODY);
            var scope_modifier_category_name = def_body[0].PartitionByRegexMatch(SCOPE_MODIFIER_CATEGORY_NAME);
            Scope = scope_modifier_category_name[0];
            Modifier = scope_modifier_category_name[1];
            TypeCategory = scope_modifier_category_name[2];
            ClassName = scope_modifier_category_name[3];
            ClassBody = def_body[1].PartitionByFirstRegexMatch(BODY_REGEX);

            if (ApplyFilter())
            {
                var executionState = ExecutePort(LOOP_PORT_NAME);
                if (executionState == ExecutionState.Failed)
                {
                    ExecutionState = ExecutionState.Failed;
                    return;
                }
            }

            if (ExecutionState != ExecutionState.Skipped)
            {
                PartitionClasses(ClassBody);
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

            ClassModifierFilter = GetPortValue(MODIFIER_FILTER_PORT_NAME, ClassModifierFilter);
            var modifierEnum = ClassModifier.None;
            if (Modifier != null)
            {
                switch (Modifier.data)
                {
                    case "static":
                        modifierEnum = ClassModifier.Static;
                        break;
                    case "sealed":
                        modifierEnum = ClassModifier.Sealed;
                        break;
                    case "abstract":
                        modifierEnum = ClassModifier.Abstract;
                        break;
                }
            }
            if ((modifierEnum & ClassModifierFilter) == 0)
            {
                return false;
            }

            TypeCategoryFilter = GetPortValue(CATEGORY_FILTER_PORT_NAME, TypeCategoryFilter);
            var categoryEnum = RefactorGraph.TypeCategory.Class;
            switch (TypeCategory.data)
            {
                case "class":
                    categoryEnum = RefactorGraph.TypeCategory.Class;
                    break;
                case "struct":
                    categoryEnum = RefactorGraph.TypeCategory.Struct;
                    break;
                case "interface":
                    categoryEnum = RefactorGraph.TypeCategory.Interface;
                    break;
                case "enum":
                    categoryEnum = RefactorGraph.TypeCategory.Enum;
                    break;
            }
            if ((categoryEnum & TypeCategoryFilter) == 0)
            {
                return false;
            }

            ClassNameFilter = GetPortValue(ClASS_NAME_FILTER_PORT_NAME, ClassNameFilter);
            if (!Partition.IsMatch(ClassName, ClassNameFilter))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}