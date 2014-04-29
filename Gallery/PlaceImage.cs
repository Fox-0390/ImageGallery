using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;

namespace Gallery
{
    public class PlaceImage : Control
    {

        public PlaceImage()
        {
            Template = (ControlTemplate)XamlReader.Load(TemplateString);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Unhook from old elements

            if (null != _desireImage)
            {
                _desireImage.ImageOpened -= new EventHandler<RoutedEventArgs>(ImageOpenedOrDownloadCompleted);
            }
            _placeHolder = GetTemplateChild("PATH_WaitStoryboard") as Storyboard;
            _placeHolder.Begin();

            // Get template parts
            _desireImage = GetTemplateChild("PATH_DesireImage") as Image;
            _failedImage = GetTemplateChild("PATH_FailedImage") as Image;
            _canvasLoader = GetTemplateChild("PATH_CanvasLoader1") as Canvas;
            _previewImage = GetTemplateChild("PATH_Preview") as Image;

           


            if (null != _previewImage)
            {
                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                {
                    _previewImage.SetBinding(Image.SourceProperty, new Binding("PreviewSource") { Source = this });
                }
                //   _desireImage.SetBinding(Image.StretchProperty, new Binding("Stretch") { Source = this });
                if (PreviewSource == null)
                {
                    _previewImage.Source = new BitmapImage(new Uri("1.png", UriKind.Relative));
                }

                _previewImage.Visibility = Visibility.Visible;

                _previewImage.ImageOpened += previewImageOpenedOrDownloadCompleted;
                //Failed PreviewImage. 
                // _previewImage.ImageFailed += _desireImage_ImageFailed;

            }

            // Set Bindings and hook up to new elements

            if (null != _desireImage)
            {
                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                {
                    _desireImage.SetBinding(Image.SourceProperty, new Binding("Source") { Source = this });
                }


                _desireImage.SetBinding(Image.StretchProperty, new Binding("StretchDesireImage") { Source = this });
                
                _desireImage.ImageOpened += ImageOpenedOrDownloadCompleted;
                //Failed Image
                _desireImage.ImageFailed += _desireImage_ImageFailed;

            }




            if (null != _failedImage)
            {

                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    if (ImageFailedSource != null)
                    {
                        _failedImage.SetBinding(Image.SourceProperty, new Binding("ImageFailedSource") { Source = this });
                    }
                    else
                    {
                        _failedImage.Source = new BitmapImage(new Uri("NoImage.png", UriKind.Relative));
                      
                    }

                _failedImage.ImageOpened += _failedImage_ImageOpened;
            }



         
        }

        void _failedImage_ImageOpened(object sender, RoutedEventArgs e)
        {


            //Какое нибудь действие в event картинки нет ? true:false;
        }

        private void previewImageOpenedOrDownloadCompleted(object sender, RoutedEventArgs e)
        {
            //_previewImage.Visibility = Visibility.;
        }

        void _desireImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _failedImage.Visibility = Visibility.Visible;
            HidePreviewState();
           
        }


        private void HidePreviewState()
        {
            _placeHolder.Stop();
            _canvasLoader.Visibility = Visibility.Collapsed;
            _previewImage.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the ImageOpened or DownloadCompleted event for the front image.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void ImageOpenedOrDownloadCompleted(object sender, EventArgs e)
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            var startPage = frame.Content as PhoneApplicationPage;

            if ((_desireImage.ActualHeight>startPage.ActualHeight||_desireImage.ActualWidth>startPage.ActualWidth) && (this.StretchDesireImage==Stretch.None))
            {
                this.StretchDesireImage = Stretch.Uniform;
            }
         
            HidePreviewState();
          
        }


       


        public static string TemplateString
        {
            get
            {
                return
                    "<ControlTemplate " +
                        "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                        "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +

                        "<Grid>" +
                               "<Grid.Resources>" +
                            "<Storyboard x:Name=\"PATH_WaitStoryboard\"	RepeatBehavior=\"Forever\">"
                            + "<DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(CompositeTransform.Rotation)\" Storyboard.TargetName=\"PATH_CanvasLoader1\">" +
                                    "<EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\" />" +
                                    "<EasingDoubleKeyFrame KeyTime=\"0:0:1.5\" Value=\"360\" />" +
                                "</DoubleAnimationUsingKeyFrames>" +
                            "</Storyboard>" +
                             "</Grid.Resources>" +

                              "<Image x:Name=\"PATH_PlaceHolder\"/>" +
                              "<Image x:Name=\"PATH_Preview\"/>" +
                              "<Image x:Name=\"PATH_DesireImage\">" +
                              "<Image.RenderTransform>" +
                            "<CompositeTransform/>" +
                            "</Image.RenderTransform>" +
                            "</Image>" +
                              "<Image x:Name=\"PATH_FailedImage\" Visibility=\"Collapsed\"/>" +
                              "<Canvas x:Name=\"PATH_CanvasLoader1\" 	Visibility=\"Visible\" RenderTransformOrigin=\"0.5,0.5\" Width=\"150\"  Background=\"Transparent\" Height=\"150\">" +

                                     " <Canvas.RenderTransform>" +
                                        "<CompositeTransform />" +
                                    "</Canvas.RenderTransform>" +
                                    "<Ellipse 	Opacity=\"1\"	    Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"61\"  Canvas.Top=\"1\"  Width=\"29\" />" +
                                    "<Ellipse 	Opacity=\".875\"	Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"17\"  Canvas.Top=\"17\" Width=\"29\" />" +
                                    "<Ellipse 	Opacity=\"0.75\"    Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"1\"   Canvas.Top=\"61\" Width=\"29\" />" +
                                    "<Ellipse 	Opacity=\"0.625\"	Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"17\"  Canvas.Top=\"104\" Width=\"29\" />" +
                                     "<Ellipse 	Opacity=\"0.5\"  	Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"61\"  Canvas.Top=\"120\" Width=\"29\" />" +
                                     "<Ellipse 	Opacity=\"0.375\"	Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"104\" Canvas.Top=\"104\" Width=\"29\" />" +
                                     "<Ellipse 	Opacity=\".25\"	    Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"120\" Canvas.Top=\"61\" Width=\"29\" />" +
                                    "<Ellipse 	Opacity=\"0.125\"	Fill=\"Gray\"  Height=\"30\" Canvas.Left=\"104\" Canvas.Top=\"17\" Width=\"29\" />" +
                             "</Canvas>" +
                        "</Grid>" +
                    "</ControlTemplate>";
            }
        }



        /// <summary>
        /// Stores a reference to the placeHolder image (placeholder image).
        /// </summary>
        //    private Image _placeHolder;

        /// <summary>
        /// Stores a reference to the preview image (desired image).
        /// </summary>
        private Image _previewImage;

        /// <summary>
        /// Stores a reference to the desire image (placeholder image).
        /// </summary>
        private Image _desireImage;

        /// <summary>
        /// Stores a reference to the failed image (desired image).
        /// </summary>
        private Image _failedImage;

        /// <summary>
        /// Canvas storyboard 
        /// </summary>
        private Storyboard _placeHolder;
        /// <summary>
        /// CanvasLoader
        /// </summary>
        private Canvas _canvasLoader;

       




        #region Desire ImageSource-Source
        /// <summary>
        /// Gets or sets the ImageSource for the desired image.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(PlaceImage), new PropertyMetadata(OnSourcePropertyChanged));
        /// <summary>
        /// Called when the Source dependency property changes.
        /// </summary>
        /// <param name="o">Event object.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PlaceImage)o).OnSourcePropertyChanged((ImageSource)e.OldValue, (ImageSource)e.NewValue);
        }


        private void OnSourcePropertyChanged(ImageSource oldValue, ImageSource newValue)
        {

            // Avoid warning about unused parameters
            oldValue = newValue;
            newValue = oldValue;
     
        }
        #endregion

        #region PreviewImageSource
        public ImageSource PreviewSource
        {
            get { return (ImageSource)GetValue(PreviewSourceProperty); }
            set { SetValue(PreviewSourceProperty, value); }
        }
        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewSourceProperty = DependencyProperty.Register("PreviewSourceProperty", typeof(ImageSource), typeof(PlaceImage), new PropertyMetadata(OnPreviewSourcePropertyPropertyChanged));
        /// <summary>
        /// Called when the Source dependency property changes.
        /// </summary>
        /// <param name="o">Event object.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnPreviewSourcePropertyPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PlaceImage)o).OnPreviewSourcePropertyChanged((ImageSource)e.OldValue, (ImageSource)e.NewValue);
        }

        private void OnPreviewSourcePropertyChanged(ImageSource imageSource1, ImageSource imageSource2)
        {
            // throw new NotImplementedException();
        }
        #endregion


        #region ImageFailed Property
        /// <summary>
        /// Image Failed
        /// </summary>
        public ImageSource ImageFailedSource
        {
            get { return (ImageSource)GetValue(ImageFailedSourceProperty); }
            set { SetValue(ImageFailedSourceProperty, value); }
        }
        /// <summary>
        /// Identifies the PlaceholderSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageFailedSourceProperty = DependencyProperty.Register("ImageFailedSource", typeof(ImageSource), typeof(PlaceImage), new PropertyMetadata(ImageFailedSourcePropertyPropertyChanged));

        private static void ImageFailedSourcePropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        #endregion



        /// <summary>
        /// stretch desire Image 
        /// </summary>
        public Stretch StretchDesireImage
        {
            get { return (Stretch)GetValue(StretchDesireImageProperty); }
            set { SetValue(StretchDesireImageProperty, value); }
        }
        /// <summary>
        /// Identifies the PlaceholderSource dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchDesireImageProperty = DependencyProperty.Register("StretchDesireImage", typeof(Stretch), typeof(PlaceImage), new PropertyMetadata(StretchDesireImagePropertyChanged));

        private static void StretchDesireImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }





    }
}
