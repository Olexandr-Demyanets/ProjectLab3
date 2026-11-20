using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectLab3.Models;
using ProjectLab3.Services;

namespace ProjectLab3.Views
{
    public partial class EditStudentPage : ContentPage
    {
        private readonly List<string> scholarshipOptions = new List<string> { "немає", "звичайна", "підвищена" };

        private readonly Student student;

        private readonly TaskCompletionSource<Student?> resultSource =
            new TaskCompletionSource<Student?>(TaskCreationOptions.RunContinuationsAsynchronously);

        public EditStudentPage(Student student, string title)
        {
            InitializeComponent();

            this.student = student;
            TitleLabel.Text = title + " (№ " + student.Id + ")";
            FillFields();
        }

        public static async Task<Student?> ShowAsync(INavigation navigation, Student student, string title)
        {
            EditStudentPage page = new EditStudentPage(student, title);
            await navigation.PushModalAsync(page);
            return await page.resultSource.Task;
        }

        private void FillFields()
        {
            FullNameEntry.Text = student.FullName;
            FacultyEntry.Text = student.Faculty;
            DepartmentEntry.Text = student.Department;
            GroupEntry.Text = student.Group;
            CourseEntry.Text = student.Course.ToString();
            BudgetCheck.IsChecked = student.IsBudget;
            HeadmanCheck.IsChecked = student.IsHeadman;
            DisciplinesEditor.Text = DisciplineTextConverter.ToText(student.Disciplines);

            if (student.Scholarship != null && !scholarshipOptions.Contains(student.Scholarship))
            {
                scholarshipOptions.Add(student.Scholarship);
            }

            ScholarshipPicker.ItemsSource = scholarshipOptions;
            ScholarshipPicker.SelectedIndex = student.Scholarship == null
                ? 0
                : scholarshipOptions.IndexOf(student.Scholarship);
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            student.FullName = (FullNameEntry.Text ?? "").Trim();
            student.Faculty = (FacultyEntry.Text ?? "").Trim();
            student.Department = (DepartmentEntry.Text ?? "").Trim();
            student.Group = (GroupEntry.Text ?? "").Trim();
            student.Course = int.TryParse(CourseEntry.Text, out int course) ? course : 0;
            student.IsBudget = BudgetCheck.IsChecked;
            student.IsHeadman = HeadmanCheck.IsChecked;
            student.Scholarship = ScholarshipPicker.SelectedIndex <= 0
                ? null
                : ScholarshipPicker.SelectedItem as string;

            try
            {
                student.Disciplines = DisciplineTextConverter.FromText(DisciplinesEditor.Text ?? "");
            }
            catch (FormatException ex)
            {
                ShowErrors(new List<string> { ex.Message });
                return;
            }

            List<string> errors = StudentValidator.Validate(student);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                return;
            }

            resultSource.TrySetResult(student);
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            resultSource.TrySetResult(null);
            await Navigation.PopModalAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            resultSource.TrySetResult(null);
        }

        private void ShowErrors(List<string> errors)
        {
            ErrorLabel.Text = string.Join("\n", errors);
            ErrorLabel.IsVisible = true;
        }
    }
}
