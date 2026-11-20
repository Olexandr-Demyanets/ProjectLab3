using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ProjectLab3.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Faculty { get; set; } = "";
        public string Department { get; set; } = "";
        public int Course { get; set; }
        public string Group { get; set; } = "";
        public bool IsBudget { get; set; }
        public string? Scholarship { get; set; }
        public bool IsHeadman { get; set; }
        public List<Discipline> Disciplines { get; set; } = new List<Discipline>();

        [JsonIgnore]
        public double AverageGrade
        {
            get
            {
                if (Disciplines.Count == 0)
                {
                    return 0;
                }
                return Disciplines.Average(d => d.Grade);
            }
        }

        [JsonIgnore]
        public string FormOfStudy
        {
            get { return IsBudget ? "бюджет" : "контракт"; }
        }

        [JsonIgnore]
        public string ScholarshipText
        {
            get { return Scholarship ?? "—"; }
        }

        [JsonIgnore]
        public string NameWithRole
        {
            get { return IsHeadman ? FullName + " (староста)" : FullName; }
        }

        public Student Clone()
        {
            Student copy = new Student
            {
                Id = Id,
                FullName = FullName,
                Faculty = Faculty,
                Department = Department,
                Course = Course,
                Group = Group,
                IsBudget = IsBudget,
                Scholarship = Scholarship,
                IsHeadman = IsHeadman
            };

            foreach (Discipline d in Disciplines)
            {
                copy.Disciplines.Add(new Discipline(d.Name, d.Grade));
            }

            return copy;
        }
    }
}
