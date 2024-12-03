using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SkillProfiAdmin.Views
{
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow(string imageUrl)
        {
            InitializeComponent();
            PreviewImage.Source = new BitmapImage(new Uri(imageUrl));
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
