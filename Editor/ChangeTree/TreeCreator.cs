using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpers.ChangeTree
{
    public class TreeCreator<Type>
    {
        //public List<TreeItem<Type>> treeItems;

        public List<TreeItem<Type>> Handle(IEnumerable<Type> data, Func<Type, int, string> getSummary, Func<Type, int> getLevel)
        {
            return Recursive(data, getSummary, getLevel);
        }

        private List<TreeItem<Type>> Recursive(IEnumerable<Type> data, Func<Type, int, string> getSummary, Func<Type, int> getLevel, int level = 1)
        {
            var treeItems = new List<TreeItem<Type>>();

            var groupedEntries = data
                .GroupBy(s =>
                {
                    return getSummary(s, level);
                }).ToList();

            foreach (var treeDataPair in groupedEntries)
            {
                string currentFolder = treeDataPair.Key;
                var treeItem = treeItems.FirstOrDefault(s => s.Summary == currentFolder);
                if (treeItem == null)
                {
                    treeItem = new TreeItem<Type>()
                    {
                        Summary = currentFolder
                    };
                }

                var deeperItems = new List<Type>();
                foreach (var childTreeItem in treeDataPair)
                {
                    var changeLevel = getLevel(childTreeItem);
                    if (level < changeLevel - 1)
                    {
                        deeperItems.Add(childTreeItem);
                    }
                    else
                    {
                        treeItem.Data.Add(childTreeItem);
                    }
                }

                if (deeperItems.Any())
                {
                    treeItem.Children = Recursive(deeperItems, getSummary, getLevel, level + 1);
                }

                treeItems.Add(treeItem);
            }

            return treeItems;
        }
    }
}