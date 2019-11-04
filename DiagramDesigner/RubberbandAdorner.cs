using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DiagramDesigner
{
    public class RubberbandAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;
        private Pen rubberbandPen;

        private DesignerCanvas designerCanvas;

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.designerCanvas = designerCanvas;
            this.startPoint = dragStartPoint;
            rubberbandPen = new Pen(Brushes.LightSlateGray, 1);
            rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();//在此元素上强制捕获鼠标

                endPoint = e.GetPosition(this);
                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();//如果此元素具有鼠标捕获，则释放该捕获
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);//移除装饰器

            e.Handled = true;
        }

        /// <summary>
        /// 重写OnRender方法，来指定Adorner如何绘制其外观
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this.startPoint.HasValue && this.endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, rubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
        }

        /// <summary>
        /// 更新选中的元素
        /// </summary>
        private void UpdateSelection()
        {
            designerCanvas.SelectionService.ClearSelection();

            Rect rubberBand = new Rect(startPoint.Value, endPoint.Value);//画矩形
            foreach (Control item in designerCanvas.Children)
            {
                Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);//获取指定的 Visual 的边界框矩形的并集。
                Rect itemBounds = item.TransformToAncestor(designerCanvas).TransformBounds(itemRect);//变换指定的边界框，并返回一个正好能容纳它的与坐标轴对齐的边界框

                if (rubberBand.Contains(itemBounds))//选择区域包含控件的边框时
                {
                    if (item is Connection)
                        designerCanvas.SelectionService.AddToSelection(item as ISelectable);
                    else
                    {
                        DesignerItem di = item as DesignerItem;
                        if (di.ParentID == Guid.Empty)
                            designerCanvas.SelectionService.AddToSelection(di);
                    }
                }
            }
        }
    }
}
