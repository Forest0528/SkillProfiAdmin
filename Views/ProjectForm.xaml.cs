using System.Windows;
using SkillProfiCRM.Models;

namespace SkillProfiAdmin.Views
{
    public partial class ProjectForm : Window
    {
        public Project CurrentProject { get; private set; }

        public ProjectForm(Project project = null)
        {
            InitializeComponent();
            CurrentProject = project ?? new Project();
            DataContext = CurrentProject;

            if (project != null)
            {
                TitleTextBox.Text = project.Title;
                DescriptionTextBox.Text = project.Description;
                ImageUrlTextBox.Text = project.ImageUrl;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || string.IsNullOrWhiteSpace(ImageUrlTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            CurrentProject.Title = TitleTextBox.Text;
            CurrentProject.Description = DescriptionTextBox.Text;
            CurrentProject.ImageUrl = ImageUrlTextBox.Text;

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
