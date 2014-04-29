using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalleryApp.Resources;
using GalleryApp.ViewModels;

namespace GalleryApp
{

  

    public partial class MainPage : PhoneApplicationPage
    {
     
        public MainPage()
        {
            InitializeComponent();



            Loaded += MainPage_Loaded;
          
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            NewsItemImages NII = new NewsItemImages();
          


   
          
            for (int i = 1; i < 10; i++)
			{
              
                NII.Images.Add(new ImagesShow { SmallUri = "/Images/preview.png", Url = String.Format("Images/1/{0}.jpg",i), Description = String.Format("{0}.jpg", i) });

			}

            flip.DataContext = NII;
        }
        }
}