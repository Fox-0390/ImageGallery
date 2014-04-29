using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gallery
{
    [TemplatePart(Name = "ContentPresenter", Type = typeof(ContentPresenter))]

    public class FlipViewItem : ContentControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.FlipViewItem" /> class.
        /// </summary>
        public FlipViewItem()
        {
            DefaultStyleKey = typeof(FlipViewItem);
            Unloaded += FlipViewItem_Unloaded;

        }

        void FlipViewItem_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= FlipViewItem_Unloaded;
            Debug.WriteLine("Unload");
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            //Set clip, so when scaling image, image won't go outside its bounds.
            try
            {

                Clip = new RectangleGeometry() { Rect = new Rect(new Point(), availableSize) };
                return base.MeasureOverride(availableSize);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetType());
                Debug.WriteLine(ex.Message);
                throw;
            }
        }










        #region IsSelected

        /// <summary>
        /// Gets or sets a value that indicates whether the item is selected.
        /// </summary>
        /// 
        /// <returns>
        /// True if the item is selected; otherwise, false.
        /// </returns>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipViewItem.IsSelected"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.FlipViewItem.IsSelected"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsSelectedProperty = FlipView.IsSelectedProperty;

        #endregion

        internal FlipView ParentFlipView { get; set; }

        internal object Item { get; set; }

        internal void OnIsSelectedChanged(bool newValue)
        {
            if (ParentFlipView != null)
            {
                ParentFlipView.NotifyItemSelected(this, newValue);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }








    }
}
