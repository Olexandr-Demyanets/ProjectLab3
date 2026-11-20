using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ProjectLab3.Models;

namespace ProjectLab3.Services
{
    public class StudentJsonService
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public string Serialize(List<Student> students)
        {
            return JsonSerializer.Serialize(students, options);
        }

        public List<Student> Deserialize(string json)
        {
            List<Student>? students = JsonSerializer.Deserialize<List<Student>>(json, options);

            if (students == null)
            {
                throw new JsonException("Файл не містить списку студентів.");
            }

            foreach (Student s in students)
            {
                if (s.FullName == null) s.FullName = "";
                if (s.Faculty == null) s.Faculty = "";
                if (s.Department == null) s.Department = "";
                if (s.Group == null) s.Group = "";
                if (s.Disciplines == null) s.Disciplines = new List<Discipline>();
            }

            return students;
        }

        public List<Student> Load(string path)
        {
            string json = File.ReadAllText(path);
            return Deserialize(json);
        }

        public void Save(string path, List<Student> students)
        {
            string json = Serialize(students);
            File.WriteAllText(path, json);
        }
    }
}
