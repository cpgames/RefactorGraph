using System;
using NodeGraph.Model;
using PCRE;

namespace RefactorGraph.Nodes.FunctionOperations
{
    [Node]
    [RefactorNode(RefactorNodeGroup.PartitionOperations, RefactorNodeType.PartitionByClasses)]
    [NodeFlowPort(COMPLETED_PORT_NAME, "Completed", false)]
    [NodeFlowPort(LOOP_PORT_NAME, "Loop", false)]
    public class PartitionByClassesNode : RefactorNodeBase
    {
        #region Fields
        public const string LOOP_PORT_NAME = "Loop";
        public const string COMPLETED_PORT_NAME = "Completed";
        public const string SOURCE_PORT_NAME = "Source";

        public const string SCOPE_FILTER_PORT_NAME = "ScopeFilter";
        public const string QUALIFIER_FILTER_PORT_NAME = "QualifierFilter";
        public const string ClASS_NAME_FILTER_PORT_NAME = "ClassNameFilterRegex";

        public const string SCOPE_PORT_NAME = "Scope";
        public const string QUALIFIER_PORT_NAME = "Qualifier";
        public const string ClASS_NAME_PORT_NAME = "ClassName";
        public const string CLASS_BODY_PORT_NAME = "ClassBody";

        private const string CLASS_DEFINITION_REGEX = @"(?:public\s*|private\s*|internal\s*)?(?:abstract\s*|static\s*)?(?:class\s*)[\s\S]*?(?=\s*{)";
        private const string SCOPE_REGEX = @"(?:public|private|internal)";
        private const string QUALIFIER_REGEX = @"(?:static|abstract)";
        private const string CLASS_NAME_WITH_GENERICS_REGEX = @"\b[\w\n\s,<>]*[\w>](?=\s*:*)*";
        private const string CLASS_NAME_REGEX = @"\w+";
        private const string GENERICS_BLOCK_REGEX = @"(?<=<)\s*\K.*\S(?=\s*>)";
        private const string GENERIC_ARGS_REGEX = @"[^,\s*]\S[\w\s_<>.()]*";
        private const string CLASS_BODY_BLOCK_REGEX = @"\s*{(?:[^{}]+|(?R))*+}";
        private const string CLASS_BODY_REGEX = @"(?<={)\s*\K\S[\S\s]+(?<!\s)(?=\s*})";

        [NodePropertyPort(SOURCE_PORT_NAME, true, typeof(Partition), null, false)]
        public Partition Source;

        [NodePropertyPort(SCOPE_FILTER_PORT_NAME, true, typeof(Scope), RefactorGraph.Scope.Any, true)]
        public Scope ScopeFilter;

        [NodePropertyPort(QUALIFIER_FILTER_PORT_NAME, true, typeof(Qualifier), RefactorGraph.Qualifier.Any, true)]
        public Qualifier QualifierFilter;

        [NodePropertyPort(ClASS_NAME_FILTER_PORT_NAME, true, typeof(string), "", true)]
        public string ClassNameFilterRegex;

        [NodePropertyPort(SCOPE_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Scope;

        [NodePropertyPort(QUALIFIER_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition Qualifier;

        [NodePropertyPort(ClASS_NAME_PORT_NAME, false, typeof(Partition), null, false)]
        public Partition ClassName;

        [NodePropertyPort(CLASS_BODY_PORT_NAME, false, typeof(Partition), null, true)]
        public Partition ClassBody;
        #endregion

        #region Properties
        protected override bool HasOutput => false;
        #endregion

        #region Constructors
        public PartitionByClassesNode(Guid guid, FlowChart flowChart) : base(guid, flowChart) { }
        #endregion

        #region Methods
        public override void OnExecute(Connector connector)
        {
            base.OnExecute(connector);

            Source = GetPortValue<Partition>(SOURCE_PORT_NAME);
            if (Source != null && !Source.IsPartitioned)
            {
                PartitionClass();
            }
        }

        private void PartitionClass()
        {
            var classDefinitions = Source.PartitionByAllRegexMatches(CLASS_DEFINITION_REGEX, PcreOptions.MultiLine);
            foreach (var classDefinition in classDefinitions)
            {
                if (!PartitionClassDefinition(classDefinition))
                {
                    continue;
                }
                var next = classDefinition.next;
                var classBodyBlock = next?.PartitionByFirstRegexMatch(CLASS_BODY_BLOCK_REGEX, PcreOptions.MultiLine);
                if (classBodyBlock == null)
                {
                    continue;
                }
                ClassBody = classBodyBlock.PartitionByFirstRegexMatch(CLASS_BODY_REGEX, PcreOptions.MultiLine);
                if (ClassBody == null)
                {
                    continue;
                }
                if (ApplyFilter())
                {
                    ExecutePort(LOOP_PORT_NAME);
                }
            }
        }

        private bool PartitionClassDefinition(Partition classDefinition)
        {
            var cur = classDefinition;
            Scope = cur.PartitionByFirstRegexMatch(SCOPE_REGEX, PcreOptions.MultiLine);
            if (Scope != null)
            {
                cur = Scope.next;
            }
            if (cur == null)
            {
                return false;
            }
            Qualifier = cur.PartitionByFirstRegexMatch(QUALIFIER_REGEX, PcreOptions.MultiLine);
            if (Qualifier != null)
            {
                cur = Qualifier.next;
            }
            if (cur == null)
            {
                return false;
            }
            var classNameWithGenerics = cur.PartitionByFirstRegexMatch(CLASS_NAME_WITH_GENERICS_REGEX, PcreOptions.MultiLine);
            if (classNameWithGenerics == null)
            {
                return false;
            }
            cur = classNameWithGenerics;
            ClassName = cur.PartitionByFirstRegexMatch(CLASS_NAME_REGEX, PcreOptions.MultiLine);
            return true;
        }

        private bool ApplyFilter()
        {
            ScopeFilter = GetPortValue(SCOPE_FILTER_PORT_NAME, ScopeFilter);
            switch (ScopeFilter)
            {
                case RefactorGraph.Scope.None:
                    if (Scope != null)
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Public:
                    if (Scope == null || Scope.Data != "public")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Private:
                    if (Scope == null || Scope.Data != "private")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Scope.Internal:
                    if (Scope == null || Scope.Data != "internal")
                    {
                        return false;
                    }
                    break;
            }

            QualifierFilter = GetPortValue(QUALIFIER_FILTER_PORT_NAME, QualifierFilter);
            switch (QualifierFilter)
            {
                case RefactorGraph.Qualifier.None:
                    if (Qualifier != null)
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Qualifier.Static:
                    if (Qualifier == null || Qualifier.Data != "static")
                    {
                        return false;
                    }
                    break;
                case RefactorGraph.Qualifier.Abstract:
                    if (Qualifier == null || Qualifier.Data != "abstract")
                    {
                        return false;
                    }
                    break;
            }

            ClassNameFilterRegex = GetPortValue(ClASS_NAME_FILTER_PORT_NAME, ClassNameFilterRegex);
            if (!string.IsNullOrEmpty(ClassNameFilterRegex))
            {
                return PcreRegex.IsMatch(ClassName.Data, ClassNameFilterRegex);
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