using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagramDesigner
{
    internal class SelectionService
    {
        private DesignerCanvas designerCanvas;

        /// <summary>
        /// 当前被选中的元素
        /// </summary>
        private List<ISelectable> currentSelection;
        internal List<ISelectable> CurrentSelection
        {
            get
            {
                if (currentSelection == null)
                    currentSelection = new List<ISelectable>();

                return currentSelection;
            }
        }

        /// <summary>
        /// 构造函数初始化
        /// </summary>
        /// <param name="canvas"></param>
        public SelectionService(DesignerCanvas canvas)
        {
            this.designerCanvas = canvas;
        }

        internal void SelectItem(ISelectable item)
        {
            this.ClearSelection();
            this.AddToSelection(item);
        }

        /// <summary>
        /// 将元素添加到选中集合中
        /// </summary>
        /// <param name="item"></param>
        internal void AddToSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);

                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = true;
                    CurrentSelection.Add(groupItem);
                }
            }
            else
            {
                item.IsSelected = true;
                CurrentSelection.Add(item);
            }
        }

        /// <summary>
        /// 将元素从选中集合中移除
        /// </summary>
        /// <param name="item"></param>
        internal void RemoveFromSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);

                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = false;
                    CurrentSelection.Remove(groupItem);
                }
            }
            else
            {
                item.IsSelected = false;
                CurrentSelection.Remove(item);
            }
        }

        /// <summary>
        /// 清空选中元素
        /// </summary>
        internal void ClearSelection()
        {
            CurrentSelection.ForEach(item => item.IsSelected = false);
            CurrentSelection.Clear();
        }

        /// <summary>
        /// 选中所有元素
        /// </summary>
        internal void SelectAll()
        {
            ClearSelection();
            CurrentSelection.AddRange(designerCanvas.Children.OfType<ISelectable>());
            CurrentSelection.ForEach(item => item.IsSelected = true);
        }

        /// <summary>
        /// 获取所有的组合
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal List<IGroupable> GetGroupMembers(IGroupable item)
        {
            IEnumerable<IGroupable> list = designerCanvas.Children.OfType<IGroupable>();
            IGroupable rootItem = GetRoot(list, item);
            return GetGroupMembers(list, rootItem);
        }

        internal IGroupable GetGroupRoot(IGroupable item)
        {
            IEnumerable<IGroupable> list = designerCanvas.Children.OfType<IGroupable>();
            return GetRoot(list, item);
        }

        /// <summary>
        /// 获取组合中的根元素
        /// </summary>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private IGroupable GetRoot(IEnumerable<IGroupable> list, IGroupable node)
        {
            if (node == null || node.ParentID == Guid.Empty)
            {
                return node;
            }
            else
            {
                foreach (IGroupable item in list)
                {
                    if (item.ID == node.ParentID)
                    {
                        return GetRoot(list, item);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 获取组内的所有元素
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<IGroupable> GetGroupMembers(IEnumerable<IGroupable> list, IGroupable parent)
        {
            List<IGroupable> groupMembers = new List<IGroupable>();
            groupMembers.Add(parent);

            var children = list.Where(node => node.ParentID == parent.ID);

            foreach (IGroupable child in children)
            {
                groupMembers.AddRange(GetGroupMembers(list, child));
            }

            return groupMembers;
        }
    }
}
