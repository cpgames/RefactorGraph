using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByClass)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class PartitionByClassNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string MODIFIER_FILTER_PORT_NAME = "ClassModifierFilter";
        public const string CATEGORY_FILTER_PORT_NAME = "TypeCategoryFilter";
        public const string ClASS_NAME_FILTER_PORT_NAME = "ClassNameFilterRegex";

        public const string CLASS_BLOCK_PORT_NAME = "ClassBlock";
        public const string SCOPE_PORT_NAME = "Scope";
        public const string MODIFIER_PORT_NAME = "ClassModifier";
        public const string CATEGORY_PORT_NAME = "TypeCategory";
        public const string ClASS_NAME_PORT_NAME = "ClassName";
        public const string CLASS_BODY_PORT_NAME = "ClassBody";

        private const string CLASS_BLOCK_REGEX = @"(?:public\s*|private\s*|protected\s*|internal\s*)?" + // scope
            @"(?:abstract\s*|static\s*|sealed\s*)?" + // modifiers
            @"(?:class\s*|interface\s*|struct\s*|enum\s*)" + // category
            @"(?:\b[\w.]+\b\s*)" + // class name
            @"(?:[\s\w:,.:]*|(<(?:[^<>]++|(?-1))*>)*)*\s*" + // details
            @"({(?:[^{}]++|(?-1))*})"; // body
        private const string CLASS_DEFINITION_REGEX = @"[\s\S]+?(?=\s*{)";
        private const string SCOPE_REGEX = @"(?:public|private|protected|internal)";
        private const string MODIFIER_REGEX = @"(?:abstract|static|sealed)";
        private const string CLASS_NAME_REGEX = @"(?:\b[\w.]+\b\s*)(<(?:[^<>]++|(?-1))*>)?";
        private const string CATEGORY_REGEX = @"(?:class|interface|struct|enum)";
        private const string CLASS_BODY_BLOCK_REGEX = @"({(?:[^{}]++|(?-1))*})";
        private const string CLASS_BODY_REGEX = @"(?<={)\s*\K\S[\S\s]+(?<!\s)(?=\s*})";

        // Inputs
        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Protected | RefactorGraph.Scope.Private | RefactorGraph.Scope.Internal | RefactorGraph.Scope.Public, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(MODIFIER_FILTER_PORT_NAME, true, typeof(ClassModifier), ClassModifier.Static | ClassModifier.Abstract | ClassModifier.None | ClassModifier.Sealed, true)]
        public ClassModifier ClassModifierFilter;

        [NodePropertyPort(CATEGORY_FILTER_PORT_NAME, true, typeof(TypeCategory), RefactorGraph.TypeCategory.Class | RefactorGraph.TypeCategory.Interface | RefactorGraph.TypeCategory.Struct | RefactorGraph.TypeCategory.Enum, true)]
        public TypeCategory TypeCategoryFilter;

        [NodePropertyPort(ClASS_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ClassNameFilterRegex;

        // Outputs
        [NodePropertyPort(CLASS_BLOCK_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition ClassBlock;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Scope;

        [NodePropertyPort(MODIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Modifier;

        [NodePropertyPort(CATEGORY_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition TypeCategory;

        [NodePropertyPort(ClASS_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition ClassName;

        [NodePropertyPort(CLASS_BODY_PORT_NAME, false, typeof(Partition), null, true)]
        public Partition ClassBody;

        private bool _somethingReturned;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        public override bool Success => _somethingReturned;
        #endregion

        #region Constructors
        public PartitionByClassNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnPreExecute(Connector prevConnector)
        {
            base.OnPreExecute(prevConnector);
            _somethingReturned = false;
        }

        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Partition.IsValidAndNotPartitioned(Source))
            {
                PartitionClass(Source);
            }
        }

        private void PartitionClass(Partition cur)
        {
            var classBlock = cur.PartitionByFirstRegexMatch(CLASS_BLOCK_REGEX, PcreOptions.MultiLine);
            if (classBlock == null)
            {
                return;
            }
            var classDef = classBlock.PartitionByFirstRegexMatch(CLASS_DEFINITION_REGEX, PcreOptions.MultiLine);
            var scope = classDef.PartitionByFirstRegexMatch(SCOPE_REGEX, PcreOptions.MultiLine);
            cur = scope != null ? scope.next : classDef;
            var modifier = cur.PartitionByFirstRegexMatch(MODIFIER_REGEX, PcreOptions.MultiLine);
            cur = modifier != null ? modifier.next : cur;
            var category = cur.PartitionByFirstRegexMatch(CATEGORY_REGEX, PcreOptions.MultiLine);
            cur = category.next;
            var className = cur.PartitionByFirstRegexMatch(CLASS_NAME_REGEX, PcreOptions.MultiLine);
            cur = classDef.next;
            var classBodyBlock = cur.PartitionByFirstRegexMatch(CLASS_BODY_BLOCK_REGEX, PcreOptions.MultiLine);
            var classBody = classBodyBlock.PartitionByFirstRegexMatch(CLASS_BODY_REGEX, PcreOptions.MultiLine);
            if (Partition.IsValid(classBody))
            {
                PartitionClass(classBody);
            }
            if (ApplyFilter(scope, modifier, category, className))
            {
                ClassBlock = classBlock;
                Scope = scope;
                Modifier = modifier;
                TypeCategory = category;
                ClassName = className;
                ClassBody = classBody;
                ExecutePort(LOOP_PORT_NAME);
                _somethingReturned = true;
            }
            cur = classBlock.next;
            if (cur != null)
            {
                PartitionClass(cur);
            }
        }

        private bool ApplyFilter(Partition scope, Partition modifier, Partition category, Partition className)
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            var scopeEnum = RefactorGraph.Scope.Scopeless;
            if (scope != null)
            {
                switch (scope.Data)
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
            if (modifier != null)
            {
                switch (modifier.Data)
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
            switch (category.Data)
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

            ClassNameFilterRegex = GetPortValue(ClASS_NAME_FILTER_PORT_NAME, ClassNameFilterRegex);
            if (!string.IsNullOrEmpty(ClassNameFilterRegex))
            {
                return PcreRegex.IsMatch(className.Data, ClassNameFilterRegex);
            }
            return true;
        }

        public override void OnPostExecute(Connector connector)
        {
            base.OnPostExecute(connector);
            ExecutePort(COMPLETED_PORT_NAME);
        }
        #endregion
    }
}