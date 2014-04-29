using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace GalleryApp.ViewModels
{
    public class ImagesShow : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Description { get; set; }
        private string shortUrl;
        public string Url
        {
            get
            {
                return shortUrl;
            //Constants.GetRootUrl(shortUrl);
            } set { shortUrl = value; } }
        public string FullUrl { get { return shortUrl; } }
        public string Size { get; set; }


        public ImagesShow()
        {
            _imagesShowGoToUrl = new DelegateCommand(ExecuteImageGoToUrl);
        }

        private void ExecuteImageGoToUrl(object obj)
        {
          //  App.ViewModel.GoToUrl(obj as string);
        }
        private ICommand _imagesShowGoToUrl;
        public ICommand ImagesShowGoToUrl
        {
            get
            {
                return _imagesShowGoToUrl;
            }
            set
            {
                if (value != null)
                {
                    _imagesShowGoToUrl = value;
                }
            }
        }

        private string _smallUri;
        public string SmallUri { get { return _smallUri; } set { if (_smallUri != value) _smallUri = value; NotifyPropertyChanged("SmallImageSource"); } }
        protected System.WeakReference bitmapImage;
        public BitmapSource SmallImageSource
        {
            get
            {
                if (bitmapImage != null)
                {
                    if (bitmapImage.IsAlive)
                    {
                        return bitmapImage.Target as BitmapSource;
                    }


                }

                if (SmallUri != null)
                    ThreadPool.QueueUserWorkItem(DownloadImage, SmallUri);

                return new WriteableBitmap(150, 150);
            }
        }

        private BitmapSource _imageSmallBitmap;
        public BitmapSource ImageSmallBitmap
        {
            get
            {
                return _imageSmallBitmap;
            }

            set
            {
                if (value != null)
                {
                    _imageSmallBitmap = value;
                }
            }
        }

        private void DownloadImage(object state)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("ReIncarnation {0}", state.ToString());
                bitmapImage = new WeakReference(new BitmapImage(new Uri(state as string)));
                NotifyPropertyChanged("SmallImageSource");
            }
                   );
        }





     


        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
