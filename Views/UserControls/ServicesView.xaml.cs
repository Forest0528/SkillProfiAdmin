using SkillProfiCRM.Models;
using System.Windows;
using System.Windows.Controls;

namespace SkillProfiAdmin.Views.UserControls
{
    public partial class ServicesView : UserControl
    {
        public ServicesView()
        {
            InitializeComponent();
            // Здесь можно инициализировать данные, например, загрузить услуги из базы данных
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика добавления новой услуги (открытие формы ServiceForm)
            ServiceForm serviceForm = new ServiceForm();
            serviceForm.Owner = Window.GetWindow(this);
            serviceForm.ShowDialog();
        }

        private void EditServiceButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика редактирования выбранной услуги
            if (ServicesDataGrid.SelectedItem is Service selectedService)
            {
                ServiceForm serviceForm = new ServiceForm(selectedService);
                serviceForm.Owner = Window.GetWindow(this);
                serviceForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите услугу для редактирования.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика удаления выбранной услуги
            if (ServicesDataGrid.SelectedItem is Service selectedService)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить услугу '{selectedService.Name}'?",
                                             "Подтверждение",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление услуги из базы данных
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите услугу для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
