using System.Collections.Generic;

namespace GitCollab.Helpers.ChangeTree
{
    public class TreeItem<Type>
    {
        public string Summary;

        public List<Type> Data;

        public List<TreeItem<Type>> Children;

        public TreeItem()
        {
            Data = new List<Type>();
            Children = new List<TreeItem<Type>>();
        }
    }
}