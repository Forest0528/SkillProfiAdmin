using SkillProfiCRM.Models;
using System.Windows;
using System.Windows.Controls;

namespace SkillProfiAdmin.Views.UserControls
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();
            // Здесь можно инициализировать данные, например, загрузить проекты из базы данных
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика добавления нового проекта (открытие формы ProjectForm)
            ProjectForm projectForm = new ProjectForm();
            projectForm.Owner = Window.GetWindow(this);
            projectForm.ShowDialog();
        }

        private void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика редактирования выбранного проекта
            if (ProjectsDataGrid.SelectedItem is Project selectedProject)
            {
                ProjectForm projectForm = new ProjectForm(selectedProject);
                projectForm.Owner = Window.GetWindow(this);
                projectForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите проект для редактирования.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика удаления выбранного проекта
            if (ProjectsDataGrid.SelectedItem is Project selectedProject)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить проект '{selectedProject.Title}'?",
                                             "Подтверждение",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление проекта из базы данных
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите проект для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ProjectsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Логика двойного клика по проекту (например, открытие окна предварительного просмотра)
            if (ProjectsDataGrid.SelectedItem is Project selectedProject)
            {
                // Открытие окна предварительного просмотра проекта
                // ImagePreviewWindow previewWindow = new ImagePreviewWindow(selectedProject.ImageUrl);
                // previewWindow.Owner = Window.GetWindow(this);
                // previewWindow.ShowDialog();
            }
        }
    }
}
