using System;
using System.Collections.Generic;
using System.Text;
using ProjectLab3.Models;

namespace ProjectLab3.Services
{
    public static class DisciplineTextConverter
    {
        public static string ToText(List<Discipline> disciplines)
        {
            StringBuilder text = new StringBuilder();

            foreach (Discipline d in disciplines)
            {
                text.AppendLine(d.Name + ": " + d.Grade);
            }

            return text.ToString().TrimEnd();
        }

        public static List<Discipline> FromText(string text)
        {
            List<Discipline> result = new List<Discipline>();
            string[] lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line == "")
                {
                    continue;
                }

                int colon = line.LastIndexOf(':');
                if (colon <= 0)
                {
                    throw new FormatException(
                        "Рядок " + (i + 1) + " «" + line + "»: потрібен формат «Назва: оцінка».");
                }

                string name = line.Substring(0, colon).Trim();
                string gradeText = line.Substring(colon + 1).Trim();

                if (!int.TryParse(gradeText, out int grade))
                {
                    throw new FormatException(
                        "Рядок " + (i + 1) + ": оцінка «" + gradeText + "» не є цілим числом.");
                }

                result.Add(new Discipline(name, grade));
            }

            return result;
        }
    }
}
