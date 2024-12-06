using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using SkillProfiAdmin.Models;
using SkillProfiCRM.Models;
using SkillProfiAdmin.Views;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Input;
using System.Text.Json;
using System.Windows.Media;
using System.ComponentModel;

namespace SkillProfiAdmin
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<Request> requests = new List<Request>();
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("https://localhost:7060/") };

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Устанавливаем контекст данных
            LoadContent("Desktop");
        }
        private void NavigateToTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                LoadContent(tag);
            }
        }
        private void LoadContent(string tag)
        {
            switch (tag)
            {
                case "Desktop":
                    MainContent.Content = new DesktopView();
                    break;
                case "Requests":
                    MainContent.Content = new RequestsView();
                    break;
                case "Services":
                    MainContent.Content = new ServicesView();
                    break;
                case "Projects":
                    MainContent.Content = new ProjectsView();
                    break;
                case "Blog":
                    MainContent.Content = new BlogView();
                    break;
                default:
                    MainContent.Content = null;
                    break;
            }
        }

        /// <summary>
        /// Универсальный метод для выполнения асинхронных действий с индикатором загрузки и обработкой ошибок.
        /// </summary>
        /// <param name="action">Асинхронное действие, которое необходимо выполнить.</param>
        private async Task ExecuteWithLoadingIndicator(Func<Task> action)
        {
            IsLoading = true;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка выполнения операции", ex);
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Универсальный метод для загрузки данных из API и привязки их к DataGrid.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="apiEndpoint">API-эндпоинт для получения данных.</param>
        /// <param name="dataGrid">DataGrid для отображения данных.</param>
        private async Task LoadData<T>(string apiEndpoint, DataGrid dataGrid)
        {
            await ExecuteWithLoadingIndicator(async () =>
            {
                try
                {
                    var data = await client.GetFromJsonAsync<List<T>>(apiEndpoint);
                    if (data != null)
                    {
                        dataGrid.ItemsSource = data;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError($"Ошибка загрузки данных с {apiEndpoint}", ex);
                    MessageBox.Show($"Не удалось загрузить данные с сервера: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    Logger.LogError($"Ошибка десериализации JSON из {apiEndpoint}", ex);
                    MessageBox.Show($"Ошибка обработки данных. Проверьте правильность JSON ответа: {ex.Message}");
                }
            });
        }


        /// <summary>
        /// Универсальный метод для сохранения или обновления данных через API.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="apiEndpoint">API-эндпоинт.</param>
        /// <param name="item">Объект данных.</param>
        /// <param name="isNew">Флаг, указывающий, является ли объект новым.</param>
        private async Task SaveOrUpdateData<T>(string apiEndpoint, T item, bool isNew)
        {
            await ExecuteWithLoadingIndicator(async () =>
            {
                HttpResponseMessage response;
                if (isNew)
                {
                    response = await client.PostAsJsonAsync(apiEndpoint, item);
                }
                else
                {
                    var idProperty = item.GetType().GetProperty("Id");
                    if (idProperty == null)
                    {
                        MessageBox.Show("Ошибка: объект не содержит свойства Id.");
                        return;
                    }

                    var id = idProperty.GetValue(item);
                    if (id == null)
                    {
                        MessageBox.Show("Ошибка: значение Id не установлено.");
                        return;
                    }

                    response = await client.PutAsJsonAsync($"{apiEndpoint}/{id}", item);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка сохранения данных: {response.StatusCode} - {errorDetails}");
                }
            });
        }

        /// <summary>
        /// Экспортирует данные в CSV-файл.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="data">Данные для экспорта.</param>
        /// <param name="fileName">Имя файла.</param>
        private void ExportToCsv<T>(IEnumerable<T> data, string fileName)
        {
            try
            {
                var properties = typeof(T).GetProperties();
                var csv = new StringBuilder();

                // Заголовок
                csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

                // Данные
                foreach (var item in data)
                {
                    var values = properties.Select(p => p.GetValue(item, null)?.ToString()?.Replace(",", " "));
                    csv.AppendLine(string.Join(",", values));
                }

                File.WriteAllText(fileName, csv.ToString(), Encoding.UTF8);
                MessageBox.Show($"Данные успешно экспортированы в {fileName}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при экспорте данных в CSV", ex);
                MessageBox.Show("Не удалось экспортировать данные. Попробуйте снова.");
            }
        }

        /// <summary>
        /// Обработчик изменения выбора в ComboBox фильтра по статусу.
        /// </summary>
        private void FilterStatusComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (requests == null) return;

            var selectedStatus = (FilterStatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "Все")
            {
                RequestsDataGrid.ItemsSource = requests;
                return;
            }

            RequestsDataGrid.ItemsSource = requests.Where(r => r.Status.ToString() == selectedStatus).ToList();
        }

        /// <summary>
        /// Обработчик изменения выбора в DataGrid заявок.
        /// </summary>
        private void RequestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem is Request selectedRequest)
            {
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == selectedRequest.Status.ToString())
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки обновления заявок.
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadRequests();
        }

        /// <summary>
        /// Обработчик кнопки обновления статуса заявки.
        /// </summary>
        private async void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem is Request selectedRequest)
            {
                try
                {
                    if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        // Парсим выбранный статус из ComboBox и преобразуем в RequestStatus
                        RequestStatus requestStatus = (RequestStatus)Enum.Parse(typeof(RequestStatus), selectedItem.Content.ToString());
                        selectedRequest.Status = requestStatus; // Здесь тип должен быть RequestStatus, а не string
                    }

                    Console.WriteLine($"Updating request ID: {selectedRequest.Id}, Status: {selectedRequest.Status}");
                    Console.WriteLine($"Request JSON: {JsonSerializer.Serialize(selectedRequest)}");

                    var response = await client.PutAsJsonAsync($"api/Request/{selectedRequest.Id}", selectedRequest);

                    Console.WriteLine($"Response Status Code: {response.StatusCode}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Статус заявки обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadRequests();
                    }
                    else
                    {
                        var errorDetails = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Ошибка обновления: {errorDetails}");
                        MessageBox.Show($"Не удалось обновить статус заявки. Ошибка: {response.StatusCode} - {errorDetails}", "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении: {ex.Message}");
                    MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для обновления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления услуги.
        /// </summary>
        private async void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var serviceForm = new ServiceForm();
            if (serviceForm.ShowDialog() == true)
            {
                await SaveOrUpdateData("api/Service", serviceForm.CurrentService, true);
                await LoadServices();
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования услуги.
        /// </summary>
        private async void EditServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem is Service selectedService)
            {
                var serviceForm = new ServiceForm(selectedService);
                if (serviceForm.ShowDialog() == true)
                {
                    await SaveOrUpdateData("api/Service", serviceForm.CurrentService, false);
                    await LoadServices();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите услугу для редактирования");
            }
        }

        /// <summary>
        /// Валидация формата Email.
        /// </summary>
        /// <param name="email">Email для проверки.</param>
        /// <returns>Возвращает true, если формат корректен; иначе false.</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка длины текста.
        /// </summary>
        /// <param name="text">Текст для проверки.</param>
        /// <param name="maxLength">Максимальная допустимая длина.</param>
        /// <param name="fieldName">Название поля для сообщения.</param>
        /// <returns>Возвращает true, если длина корректна; иначе false.</returns>
        private bool IsValidTextLength(string text, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > maxLength)
            {
                MessageBox.Show($"{fieldName} не может быть пустым или длиннее {maxLength} символов.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Обработчик кнопки удаления услуги.
        /// </summary>
        private async void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem is Service selectedService)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await client.DeleteAsync($"api/Service/{selectedService.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Услуга удалена");
                            await LoadServices();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить услугу");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите услугу для удаления");
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления проекта.
        /// </summary>
        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var projectForm = new ProjectForm();
            if (projectForm.ShowDialog() == true)
            {
                var newProject = projectForm.CurrentProject;

                try
                {
                    var response = await client.PostAsJsonAsync("api/Project", newProject);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Проект добавлен");
                        await LoadProjects();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить проект");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования проекта.
        /// </summary>
        private async void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsDataGrid.SelectedItem is Project selectedProject)
            {
                var projectForm = new ProjectForm(selectedProject);
                if (projectForm.ShowDialog() == true)
                {
                    try
                    {
                        var response = await client.PutAsJsonAsync($"api/Project/{selectedProject.Id}", projectForm.CurrentProject);
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Проект обновлен");
                            await LoadProjects();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить проект");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении проекта: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите проект для редактирования");
            }
        }

        /// <summary>
        /// Обработчик кнопки удаления проекта.
        /// </summary>
        private async void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsDataGrid.SelectedItem is Project selectedProject)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот проект?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await client.DeleteAsync($"api/Project/{selectedProject.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Проект удален");
                            await LoadProjects();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить проект");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите проект для удаления");
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления статьи в блог.
        /// </summary>
        private async void AddBlogPostButton_Click(object sender, RoutedEventArgs e)
        {
            var blogPostForm = new BlogPostForm();
            if (blogPostForm.ShowDialog() == true)
            {
                var newBlogPost = blogPostForm.CurrentBlogPost;

                try
                {
                    var response = await client.PostAsJsonAsync("api/BlogPost", newBlogPost);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Статья блога добавлена");
                        await LoadBlogPosts();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить статью блога");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении статьи блога: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования статьи в блоге.
        /// </summary>
        private async void EditBlogPostButton_Click(object sender, RoutedEventArgs e)
        {
            if (BlogPostsDataGrid.SelectedItem is BlogPost selectedBlogPost)
            {
                var blogPostForm = new BlogPostForm(selectedBlogPost);
                if (blogPostForm.ShowDialog() == true)
                {
                    try
                    {
                        var response = await client.PutAsJsonAsync($"api/BlogPost/{selectedBlogPost.Id}", blogPostForm.CurrentBlogPost);
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Статья блога обновлена");
                            await LoadBlogPosts();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить статью блога");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении статьи блога: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите статью для редактирования");
            }
        }

        /// <summary>
        /// Обработчик кнопки удаления статьи в блоге.
        /// </summary>
        private async void DeleteBlogPostButton_Click(object sender, RoutedEventArgs e)
        {
            if (BlogPostsDataGrid.SelectedItem is BlogPost selectedBlogPost)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту статью блога?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await client.DeleteAsync($"api/BlogPost/{selectedBlogPost.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Статья блога удалена");
                            await LoadBlogPosts();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить статью блога");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении статьи блога: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите статью для удаления");
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска.
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (requests == null || requests.Count == 0) return;

            var filter = SearchTextBox.Text.ToLower();
            var filteredRequests = requests
                .Where(r => r.FirstName.ToLower().Contains(filter) ||
                            r.LastName.ToLower().Contains(filter) ||
                            r.Email.ToLower().Contains(filter))
                .ToList();

            RequestsDataGrid.ItemsSource = filteredRequests;
        }

        /// <summary>
        /// Обработчик кнопки экспорта заявок в CSV.
        /// </summary>
        private void ExportRequestsToCsv_Click(object sender, RoutedEventArgs e)
        {
            if (requests == null || !requests.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "Requests.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportToCsv(requests, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Обработчик двойного клика по DataGrid проектов для просмотра изображения.
        /// </summary>
        private void ProjectsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProjectsDataGrid.SelectedItem is Project selectedProject && !string.IsNullOrEmpty(selectedProject.ImageUrl))
            {
                var previewWindow = new ImagePreviewWindow(selectedProject.ImageUrl);
                previewWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Изображение для этого проекта отсутствует.");
            }
        }

        /// <summary>
        /// Обработчик двойного клика по RequestsDataGrid для открытия предварительного просмотра заявки.
        /// </summary>
        private void RequestsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что клик был по строке
            if (RequestsDataGrid.SelectedItem is Request selectedRequest)
            {
                var previewWindow = new RequestPreviewWindow(selectedRequest);
                previewWindow.Owner = this; // Устанавливаем владельца окна
                previewWindow.ShowDialog();
            }
        }
        private void NavigateToTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tabName)
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Header.ToString() == tabName)
                    {
                        MainTabControl.SelectedItem = tab;
                        break;
                    }
                }
            }
        }
        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == "Поиск...")
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Поиск...";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }


        /// <summary>
        /// Метод загрузки заявок.
        /// </summary>
        private async Task LoadRequests() => await LoadData<Request>("api/Request", RequestsDataGrid);

        /// <summary>
        /// Метод загрузки услуг.
        /// </summary>
        private async Task LoadServices() => await LoadData<Service>("api/Service", ServicesDataGrid);

        /// <summary>
        /// Метод загрузки проектов.
        /// </summary>
        private async Task LoadProjects() => await LoadData<Project>("api/Project", ProjectsDataGrid);

        /// <summary>
        /// Метод загрузки статей блога.
        /// </summary>
        private async Task LoadBlogPosts() => await LoadData<BlogPost>("api/BlogPost", BlogPostsDataGrid);
    }
}
