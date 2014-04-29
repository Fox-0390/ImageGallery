using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GalleryApp
{
    //[TemplatePart(Name = "Thumbnail", Type = typeof(Image))]
    //[TemplatePart(Name = "Image", Type = typeof(Image))]
    //[TemplatePart(Name = "Root", Type = typeof(Grid))]
    public class CustomImage : ContentControl
    {
        private Image ImageControl;
        private Image ThumbnailImageControl;
        private Grid Root;

        private PerformanceProgressBar progressbar;

        /// <summary>
        /// Gets or sets the thumbnail. This should be the exact same as the 
        /// Image set on <see cref="Image"/>, but at a lower resolution.
        /// </summary>
        public ImageSource Thumbnail
        {
            get { return (ImageSource)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Thumbnail"/> Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(ImageSource), typeof(CustomImage), new PropertyMetadata(OnThumbnailPropertyChanged));

        private static void OnThumbnailPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var iv = (d as CustomImage);
           
            iv.IsThumbnailLoaded = false;
            if (iv.ThumbnailImageControl != null)
                iv.ThumbnailImageControl.Visibility = Visibility.Visible;
        }


        public Stretch StretchThumbnail
        {
            get { return (Stretch)GetValue(StretchThumbnailProperty); }
            set { SetValue(StretchThumbnailProperty, value); }
        }



        public static readonly DependencyProperty StretchThumbnailProperty =
           DependencyProperty.Register("StretchThumbnail", typeof(Stretch), typeof(CustomImage), new PropertyMetadata(SetStretchThumbnail));

        private static void SetStretchThumbnail(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var iv = (d as CustomImage);
            iv.StretchThumbnail =  (Stretch)e.NewValue;
        }

        





        /// <summary>
        /// Gets or sets the high resolution image to display.
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Image"/> Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(CustomImage), new PropertyMetadata(OnImagePropertyChanged));

        private static void OnImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var iv = (d as CustomImage);
            iv.IsImageLoaded = false;
        }






        public CustomImage()
        {
            DefaultStyleKey = typeof(CustomImage);
          
            
        }

        public override void OnApplyTemplate()
        {
            if (ThumbnailImageControl != null) //Unhook from old template
            {
                ThumbnailImageControl.Source = null;
                ThumbnailImageControl.ImageOpened -= ThumbImage_ImageOpened;
                ThumbnailImageControl.ImageFailed -= ThumbImage_ImageFailed;
            }
            progressbar = GetTemplateChild("progressbar") as PerformanceProgressBar;
            ThumbnailImageControl = GetTemplateChild("Thumbnail") as Image;
            if (ThumbnailImageControl != null)
            {

                //if (Thumbnail!=null&&ImageProperties.GetImageFromCache((Thumbnail as BitmapImage).UriSource) != null)
                //    ThumbnailImageControl.Source = ImageProperties.GetImageFromCache((Thumbnail as BitmapImage).UriSource);
                //    ThumbnailImageControl.ImageOpened += ThumbImage_ImageOpened;
                //    ThumbnailImageControl.ImageFailed += ThumbImage_ImageFailed;
              
            }

            if (ImageControl != null) //Unhook from old template
            {
                ImageControl.Source = null;
                ImageControl.ImageOpened -= Image_ImageOpened;
                ImageControl.ImageFailed -= Image_ImageFailed;
            }
            ImageControl = GetTemplateChild("Image") as Image;
            if (ImageControl != null)
            {
               
                ImageControl.ImageOpened += Image_ImageOpened;
                ImageControl.ImageFailed += Image_ImageFailed;
            }

            if (Root != null) //Unhook from old template
            {
                //Root.ManipulationDelta -= Root_ManipulationDelta;
                //Root.ManipulationStarted -= Root_ManipulationStarted;
                //RootRenderTransform = null;
            }
            Root = GetTemplateChild("Root") as Grid;
            if (Root != null)
            {
                //Root.ManipulationDelta += Root_ManipulationDelta;
                //Root.ManipulationStarted += Root_ManipulationStarted;
                //Root.RenderTransform = RootRenderTransform = new CompositeTransform();
            }
            base.OnApplyTemplate();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //Set clip, so when scaling image, image won't go outside its bounds.
            Clip = new RectangleGeometry() { Rect = new Rect(new Point(), availableSize) };
            return base.MeasureOverride(availableSize);
        }



        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            IsImageLoaded = true;
            progressbar.IsIndeterminate = false;
            if (ImageOpened != null)
                ImageOpened(this, EventArgs.Empty);
            if (ThumbnailImageControl != null)
            {
                //ThumbnailImageControl.Source = ImageProperties.GetImageFromCache((Thumbnail as BitmapImage).UriSource);
                //ThumbnailImageControl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

            Root.MinWidth = 300;
            
            Image = new BitmapImage(new Uri("/SputnikWP;component/icons/no_photo.png", UriKind.Relative)); 

            if (ImageFailed != null)
                ImageFailed(this, EventArgs.Empty);
        }

        private void ThumbImage_ImageOpened(object sender, RoutedEventArgs e)
        {

            IsThumbnailLoaded = true;
            if (ThumbnailOpened != null)
                ThumbnailOpened(this, EventArgs.Empty);
        }

        private void ThumbImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            progressbar.IsIndeterminate = false;
            if (ThumbnailFailed != null)
                ThumbnailFailed(this, EventArgs.Empty);
        }



        public bool IsImageLoaded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the thumbnail image has loaded.
        /// </summary>
        public bool IsThumbnailLoaded { get; private set; }

        /// <summary>
        /// Fired when the full resolution image has successfully loaded.
        /// </summary>
        public event EventHandler ImageOpened;
        /// <summary>
        /// Fired when the full resolution image failed to load.
        /// </summary>
        public event EventHandler ImageFailed;
        /// <summary>
        /// Fired when the thumbnail image has successfully loaded.
        /// </summary>
        public event EventHandler ThumbnailOpened;
        /// <summary>
        /// Fired when the thumbnail image has failed to load.
        /// </summary>
        public event EventHandler ThumbnailFailed;
    }
}
