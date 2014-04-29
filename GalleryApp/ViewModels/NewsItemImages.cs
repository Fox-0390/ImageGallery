using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using System.IO;
namespace GalleryApp.ViewModels
{
    public class NewsItemImages : NewsItem, INotifyPropertyChanged
    {


        private int _selectedImageIndex;

        public int SelectedImageIndex
        {
            get { return _selectedImageIndex; }
            set { _selectedImageIndex = value; NotifyPropertyChanged("SelectedImageIndex"); }
        }

        private ObservableCollection<ImagesShow> _images = new ObservableCollection<ImagesShow>();
        public ObservableCollection<ImagesShow> Images
        {
            get
            {
                return _images;
            }

            set
            {
                _images = value;
                NotifyPropertyChanged("Images");
            }
        }
        public NewsItemImages()
        {
            _saveImageToMediaLibrary = new DelegateCommand(ExecuteSaveImage);

        }

        private void ExecuteSaveImage(object obj)
        {
         
        }


        public Byte[] BytesFromImage(BitmapImage imageSource)
        {
            if (imageSource == null) return null;
            // var bmp= new BitmapImage(new Uri(imageSource));
            var ms = new MemoryStream();

            var wb = new WriteableBitmap(imageSource);

            wb.SaveJpeg(ms, wb.PixelWidth, wb.PixelHeight, 0, 80);
            ms.Close();

            return ms.ToArray();
        }

        private ICommand _saveImageToMediaLibrary;
        public ICommand SaveImageToMediaLibrary { get { return _saveImageToMediaLibrary; } set { if (value != null) _saveImageToMediaLibrary = value; } }

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
