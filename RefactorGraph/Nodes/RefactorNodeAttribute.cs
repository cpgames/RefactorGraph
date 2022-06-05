using System;

namespace RefactorGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RefactorNodeAttribute : Attribute
    {
        #region Fields
        public RefactorNodeGroup group;
        public RefactorNodeType nodeType;
        #endregion
    }
}