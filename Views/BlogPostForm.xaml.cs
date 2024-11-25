using System.Windows;
using SkillProfiCRM.Models;

namespace SkillProfiAdmin.Views
{
    public partial class BlogPostForm : Window
    {
        public BlogPost CurrentBlogPost { get; private set; }

        public BlogPostForm(BlogPost blogPost = null)
        {
            InitializeComponent();
            CurrentBlogPost = blogPost ?? new BlogPost();
            DataContext = CurrentBlogPost;

            // Если редактируется существующая статья, заполните поля формы
            if (blogPost != null)
            {
                TitleTextBox.Text = blogPost.Title;
                ContentTextBox.Text = blogPost.Content;
                PublishedDatePicker.SelectedDate = blogPost.PublishedDate;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка корректности заполнения полей
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || string.IsNullOrWhiteSpace(ContentTextBox.Text) || PublishedDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            CurrentBlogPost.Title = TitleTextBox.Text;
            CurrentBlogPost.Content = ContentTextBox.Text;
            CurrentBlogPost.PublishedDate = PublishedDatePicker.SelectedDate.Value;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
