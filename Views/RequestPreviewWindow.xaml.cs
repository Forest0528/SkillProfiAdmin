using System.Windows;
using SkillProfiAdmin.Models;

namespace SkillProfiAdmin.Views
{
    public partial class RequestPreviewWindow : Window
    {
        public RequestPreviewWindow(Request request)
        {
            InitializeComponent();
            this.DataContext = request;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
