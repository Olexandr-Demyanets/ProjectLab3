using System.Collections.Generic;
using ProjectLab3.Models;

namespace ProjectLab3.Services
{
    public static class StudentValidator
    {
        public static List<string> Validate(Student s)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(s.FullName))
            {
                errors.Add("Вкажіть ПІБ.");
            }

            if (string.IsNullOrWhiteSpace(s.Faculty))
            {
                errors.Add("Вкажіть факультет.");
            }

            if (string.IsNullOrWhiteSpace(s.Group))
            {
                errors.Add("Вкажіть групу.");
            }

            if (s.Course < 1 || s.Course > 6)
            {
                errors.Add("Курс має бути цілим числом від 1 до 6.");
            }

            if (s.Scholarship != null && !s.IsBudget)
            {
                errors.Add("Стипендію можна призначити лише студенту на бюджеті.");
            }

            if (s.Disciplines.Count == 0)
            {
                errors.Add("Додайте хоча б одну дисципліну.");
            }

            foreach (Discipline d in s.Disciplines)
            {
                if (string.IsNullOrWhiteSpace(d.Name))
                {
                    errors.Add("У дисципліни немає назви.");
                }

                if (d.Grade < 0 || d.Grade > 100)
                {
                    errors.Add("Оцінка з «" + d.Name + "» має бути від 0 до 100.");
                }
            }

            return errors;
        }
    }
}
