using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DiagramDesigner
{
    public class DesignerItemsControlItemStyleSelector : StyleSelector
    {
        static DesignerItemsControlItemStyleSelector()
        {
            Instance = new DesignerItemsControlItemStyleSelector();
        }

        public static DesignerItemsControlItemStyleSelector Instance
        {
            get;
            private set;
        }


        /// <summary>
        /// 给内部的控件选择特定的样式
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            //返回拥有指定容器元素的 ItemsControl
            //ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

            //if (itemsControl == null)
            //    throw new InvalidOperationException("DesignerItemsControlItemStyleSelector : Could not find ItemsControl");

            //if (item is DesignerItemViewModelBase)
            //{
            //    return (Style)itemsControl.FindResource("designerItemStyle");
            //}

            //if (item is ConnectorViewModel)
            //{
            //    return (Style)itemsControl.FindResource("connectorItemStyle");
            //}

            return null;
        }
    }
}
