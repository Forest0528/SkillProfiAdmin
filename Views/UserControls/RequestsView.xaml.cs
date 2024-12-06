using System.Windows;
using System.Windows.Controls;

namespace SkillProfiAdmin.Views.UserControls
{
    public partial class RequestsView : UserControl
    {
        public RequestsView()
        {
            InitializeComponent();
            // Здесь можно инициализировать данные, например, загрузить заявки из базы данных
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Логика поиска заявок по тексту
        }

        private void FilterStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика фильтрации заявок по статусу
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика обновления статуса выбранной заявки
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика обновления данных в DataGrid
        }

        private void ExportRequestsToCsv_Click(object sender, RoutedEventArgs e)
        {
            // Логика экспорта заявок в CSV файл
        }

        private void RequestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика обработки выбора строки в DataGrid
        }

        private void RequestsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Логика двойного клика по строке (например, открытие окна предварительного просмотра)
        }
    }
}
