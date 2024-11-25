using System.Windows;
using SkillProfiCRM.Models;

namespace SkillProfiAdmin.Views
{
    public partial class ServiceForm : Window
    {
        public Service CurrentService { get; private set; }

        public ServiceForm(Service service = null)
        {
            InitializeComponent();
            CurrentService = service ?? new Service();
            DataContext = CurrentService;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка длины названия и описания
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || NameTextBox.Text.Length > 50)
            {
                MessageBox.Show("Название услуги не может быть пустым или длиннее 50 символов.");
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || DescriptionTextBox.Text.Length > 200)
            {
                MessageBox.Show("Описание услуги не может быть пустым или длиннее 200 символов.");
                return;
            }

            CurrentService.Name = NameTextBox.Text;
            CurrentService.Description = DescriptionTextBox.Text;
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
