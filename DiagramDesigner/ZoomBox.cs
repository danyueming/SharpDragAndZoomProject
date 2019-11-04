using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DiagramDesigner
{
    public class ZoomBox : Control
    {
        private Thumb zoomThumb;//自定义控件的成员变量，在xmal中定义的
        private Canvas zoomCanvas;
        private Slider zoomSlider;
        private ScaleTransform scaleTransform;
        private double mouseOffsetX = 0.0;
        private double mouseOffsety = 0.0;


        #region 自定义依赖属性ScrollViewer

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ZoomBox));
        #endregion

        #region 自定义依赖属性DesignerCanvas

        public static readonly DependencyProperty DesignerCanvasProperty =
            DependencyProperty.Register("DesignerCanvas", typeof(DesignerCanvas), typeof(ZoomBox),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnDesignerCanvasChanged)));


        public DesignerCanvas DesignerCanvas
        {
            get { return (DesignerCanvas)GetValue(DesignerCanvasProperty); }
            set { SetValue(DesignerCanvasProperty, value); }
        }

        #endregion


        /// <summary>
        /// layout改变，更新DesignerCanvas的位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            this.zoomThumb.Width = this.ScrollViewer.ViewportWidth * scale;
            this.zoomThumb.Height = this.ScrollViewer.ViewportHeight * scale;
            Canvas.SetLeft(this.zoomThumb, xOffset + this.ScrollViewer.HorizontalOffset * scale);
            Canvas.SetTop(this.zoomThumb, yOffset + this.ScrollViewer.VerticalOffset * scale);
        }

        /// <summary>
        /// 重新计算缩放因子
        /// </summary>
        /// <param name="scale">缩放因子</param>
        /// <param name="xOffset">X偏移量</param>
        /// <param name="yOffset">Y偏移量</param>
        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            double w = DesignerCanvas.ActualWidth * this.scaleTransform.ScaleX;//DesignerCanvas缩放后的宽度
            double h = DesignerCanvas.ActualHeight * this.scaleTransform.ScaleY;//DesignerCanvas缩放后的高度

            // zoom canvas size
            double x = this.zoomCanvas.ActualWidth;//zoomCanvas的宽度
            double y = this.zoomCanvas.ActualHeight;//zoomCanvas的高度
            double scaleX = x / w;
            double scaleY = y / h;
            scale = (scaleX < scaleY) ? scaleX : scaleY;
            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }

        /// <summary>
        /// 鼠标滚轮，更改zoomSlider的值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignerCanvas_MouseWheel(object sender, EventArgs e)
        {
            MouseWheelEventArgs wheel = (MouseWheelEventArgs)e;
            //divide the value by 10 so that it is more smooth
            double value = Math.Max(0, wheel.Delta / 10);
            value = Math.Min(wheel.Delta / 12, 10);
            this.zoomSlider.Value += value;
        }

        /// <summary>
        /// (2)DesignerCanvas发生改变时，添加或者移除关联的事件
        /// </summary>
        /// <param name="oldDesignerCanvas"></param>
        /// <param name="newDesignerCanvas"></param>
        protected virtual void OnDesignerCanvasChanged(DesignerCanvas oldDesignerCanvas, DesignerCanvas newDesignerCanvas)
        {
            if (oldDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated -= new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel -= new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
            }

            if (newDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated += new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel += new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
                newDesignerCanvas.LayoutTransform = this.scaleTransform;
            }
        }


        /// <summary>
        /// (1)DesignerCanvas发生改变时
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnDesignerCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBox target = (ZoomBox)d;
            DesignerCanvas oldDesignerCanvas = (DesignerCanvas)e.OldValue;
            DesignerCanvas newDesignerCanvas = target.DesignerCanvas;
            target.OnDesignerCanvasChanged(oldDesignerCanvas, newDesignerCanvas);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.ScrollViewer == null)
                return;

            #region 搜索控件的模板元素

            this.zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (this.zoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            this.zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (this.zoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            this.zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (this.zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            #endregion

            this.zoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            this.zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
            this.scaleTransform = new ScaleTransform();
        }

        /// <summary>
        /// Slider Value Change Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //缩放因子
            double scale = e.NewValue / e.OldValue;
            double halfViewportHeight = this.ScrollViewer.ViewportHeight;
            double halfViewportWidth = this.ScrollViewer.ViewportWidth / 2;
            //垂直偏移量
            double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight) * scale - halfViewportHeight);
            //水平偏移量
            double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth) * scale - halfViewportWidth);
            this.scaleTransform.ScaleX *= scale;
            this.scaleTransform.ScaleY *= scale;
            this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        /// <summary>
        /// thumb拖动事件，将 ScrollViewer 中的内容滚动到指定的偏移位置。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange / scale);
            this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange / scale);
        }

    }

}
