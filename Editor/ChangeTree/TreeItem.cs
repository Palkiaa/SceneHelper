using System.Collections.Generic;

namespace Helpers.ChangeTree
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