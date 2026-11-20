using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ProjectLab3.Models;
using ProjectLab3.Services;
using ProjectLab3.Views;

namespace ProjectLab3
{
    public partial class MainPage : ContentPage
    {
        private readonly StudentJsonService jsonService = new StudentJsonService();
        private readonly StudentSearchService searchService = new StudentSearchService();

        private List<Student> allStudents = new List<Student>();
        private readonly ObservableCollection<Student> visibleStudents =
            new ObservableCollection<Student>();
        private StudentSearchCriteria currentCriteria = new StudentSearchCriteria();

        private string currentFilePath = "";
        private bool hasUnsavedChanges = false;
        private bool defaultLoadTried = false;

        private const string AnyGroup = "(усі групи)";
        private readonly List<string> formOptions = new List<string> { "(будь-яка форма)", "бюджет", "контракт" };

        private readonly FilePickerFileType jsonFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json" } },
                { DevicePlatform.Android, new[] { "application/json" } },
                { DevicePlatform.iOS, new[] { "public.json" } },
                { DevicePlatform.MacCatalyst, new[] { "public.json" } }
            });

        public MainPage()
        {
            InitializeComponent();

            StudentsList.ItemsSource = visibleStudents;
            FormPicker.ItemsSource = formOptions;
            FormPicker.SelectedIndex = 0;
            UpdateGroupPicker();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!defaultLoadTried)
            {
                defaultLoadTried = true;
                await LoadDefaultFileAsync();
            }
        }

        private async void OnOpenClicked(object? sender, EventArgs e)
        {
            if (!await ConfirmDiscardChangesAsync())
            {
                return;
            }

            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть JSON-файл зі студентами",
                FileTypes = jsonFileType
            });

            if (file == null)
            {
                return;
            }

            await TryLoadFileAsync(file.FullPath);
        }

        private async void OnReloadClicked(object? sender, EventArgs e)
        {
            if (currentFilePath == "")
            {
                await DisplayAlertAsync("Помилка", "Спочатку відкрийте файл.", "OK");
                return;
            }

            if (!await ConfirmDiscardChangesAsync())
            {
                return;
            }

            await TryLoadFileAsync(currentFilePath);
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (currentFilePath == "")
            {
                currentFilePath = GetDefaultFilePath();
            }

            try
            {
                jsonService.Save(currentFilePath, allStudents);
                hasUnsavedChanges = false;
                UpdateStatus();
                await DisplayAlertAsync("Збережено", "Дані збережено у файл:\n" + currentFilePath, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка", "Не вдалося зберегти файл: " + ex.Message, "OK");
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            Student newStudent = new Student
            {
                Id = searchService.GetNextId(allStudents),
                Course = 1,
                IsBudget = true
            };

            Student? result = await EditStudentPage.ShowAsync(Navigation, newStudent, "Новий студент");
            if (result == null)
            {
                return;
            }

            allStudents.Add(result);
            OnDataChanged();
        }

        private async void OnEditClicked(object? sender, EventArgs e)
        {
            if (StudentsList.SelectedItem is not Student selected)
            {
                await DisplayAlertAsync("Редагування", "Спочатку оберіть студента в таблиці.", "OK");
                return;
            }

            Student? result = await EditStudentPage.ShowAsync(Navigation, selected.Clone(), "Редагування студента");
            if (result == null)
            {
                return;
            }

            int index = allStudents.IndexOf(selected);
            allStudents[index] = result;
            OnDataChanged();
        }

        private async void OnDeleteClicked(object? sender, EventArgs e)
        {
            if (StudentsList.SelectedItem is not Student selected)
            {
                await DisplayAlertAsync("Видалення", "Спочатку оберіть студента в таблиці.", "OK");
                return;
            }

            bool confirm = await DisplayAlertAsync(
                "Видалення",
                "Видалити студента «" + selected.FullName + "»?",
                "Так",
                "Ні");

            if (!confirm)
            {
                return;
            }

            allStudents.Remove(selected);
            OnDataChanged();
        }

        private async void OnAboutClicked(object? sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AboutPage());
        }

        private async void OnSearchClicked(object? sender, EventArgs e)
        {
            StudentSearchCriteria criteria = new StudentSearchCriteria();

            criteria.NamePart = (NameEntry.Text ?? "").Trim();

            if (GroupPicker.SelectedIndex > 0)
            {
                criteria.Group = GroupPicker.SelectedItem as string;
            }

            string minText = (MinAverageEntry.Text ?? "").Trim().Replace(',', '.');
            if (minText != "")
            {
                if (!double.TryParse(minText, NumberStyles.Float, CultureInfo.InvariantCulture, out double min))
                {
                    await DisplayAlertAsync("Помилка", "Мінімальний середній бал має бути числом, наприклад 75.5", "OK");
                    return;
                }
                criteria.MinAverage = min;
            }

            if (FormPicker.SelectedIndex == 1)
            {
                criteria.IsBudget = true;
            }
            else if (FormPicker.SelectedIndex == 2)
            {
                criteria.IsBudget = false;
            }

            currentCriteria = criteria;
            RefreshTable();
        }

        private void OnResetSearchClicked(object? sender, EventArgs e)
        {
            ResetSearchFields();
            RefreshTable();
        }

        private string GetDefaultFilePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "students.json");
        }

        private async Task LoadDefaultFileAsync()
        {
            string path = GetDefaultFilePath();

            try
            {
                if (!File.Exists(path))
                {
                    using (Stream source = await FileSystem.OpenAppPackageFileAsync("students.json"))
                    using (FileStream target = File.Create(path))
                    {
                        await source.CopyToAsync(target);
                    }
                }
            }
            catch (Exception)
            {
                StatusLabel.Text = "Вбудований файл не знайдено. Відкрийте JSON-файл кнопкою «Відкрити JSON».";
                return;
            }

            await TryLoadFileAsync(path);
        }

        private async Task TryLoadFileAsync(string path)
        {
            try
            {
                allStudents = jsonService.Load(path);
                currentFilePath = path;
                hasUnsavedChanges = false;

                ResetSearchFields();
                UpdateGroupPicker();
                RefreshTable();
            }
            catch (JsonException ex)
            {
                await DisplayAlertAsync("Помилка", "Файл не є коректним JSON зі списком студентів.\n" + ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Помилка", "Не вдалося прочитати файл: " + ex.Message, "OK");
            }
        }

        private void OnDataChanged()
        {
            hasUnsavedChanges = true;
            UpdateGroupPicker();
            RefreshTable();
        }

        private void RefreshTable()
        {
            List<Student> result = searchService.Search(allStudents, currentCriteria);

            visibleStudents.Clear();
            foreach (Student s in result)
            {
                visibleStudents.Add(s);
            }

            UpdateStatus();
        }

        private void UpdateGroupPicker()
        {
            string? selected = GroupPicker.SelectedItem as string;

            List<string> items = new List<string> { AnyGroup };
            items.AddRange(searchService.GetGroups(allStudents));

            GroupPicker.ItemsSource = items;

            int index = selected == null ? 0 : items.IndexOf(selected);
            GroupPicker.SelectedIndex = index >= 0 ? index : 0;
        }

        private void ResetSearchFields()
        {
            NameEntry.Text = "";
            MinAverageEntry.Text = "";
            GroupPicker.SelectedIndex = 0;
            FormPicker.SelectedIndex = 0;
            currentCriteria = new StudentSearchCriteria();
        }

        private void UpdateStatus()
        {
            string fileName = currentFilePath == "" ? "не відкрито" : Path.GetFileName(currentFilePath);
            string text = "Файл: " + fileName
                + "  ·  показано " + visibleStudents.Count + " з " + allStudents.Count;

            if (!currentCriteria.IsEmpty)
            {
                text += "  ·  активний пошук";
            }

            if (hasUnsavedChanges)
            {
                text += "  ·  є незбережені зміни";
            }

            StatusLabel.Text = text;
        }

        private async Task<bool> ConfirmDiscardChangesAsync()
        {
            if (!hasUnsavedChanges)
            {
                return true;
            }

            return await DisplayAlertAsync(
                "Незбережені зміни",
                "Зміни не збережено у файл. Продовжити і втратити їх?",
                "Так",
                "Ні");
        }
    }
}
