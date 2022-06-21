using System;

namespace RefactorGraph.Nodes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RefactorNodeAttribute : Attribute
    {
        #region Fields
        public RefactorNodeGroup nodeGroup;
        public RefactorNodeType nodeType;
        #endregion

        #region Constructors
        public RefactorNodeAttribute(RefactorNodeGroup nodeGroup, RefactorNodeType nodeType)
        {
            this.nodeGroup = nodeGroup;
            this.nodeType = nodeType;
        }
        #endregion
    }
}