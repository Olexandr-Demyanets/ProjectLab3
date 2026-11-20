using System.Collections.Generic;
using System.Linq;
using ProjectLab3.Models;

namespace ProjectLab3.Services
{
    public class StudentSearchService
    {
        public List<Student> Search(IEnumerable<Student> students, StudentSearchCriteria criteria)
        {
            IEnumerable<Student> query = students;

            if (!string.IsNullOrWhiteSpace(criteria.NamePart))
            {
                string part = criteria.NamePart.Trim().ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(part));
            }

            if (criteria.Group != null)
            {
                string group = criteria.Group;
                query = query.Where(s => s.Group == group);
            }

            if (criteria.MinAverage != null)
            {
                double min = criteria.MinAverage.Value;
                query = query.Where(s => s.AverageGrade >= min);
            }

            if (criteria.IsBudget != null)
            {
                bool isBudget = criteria.IsBudget.Value;
                query = query.Where(s => s.IsBudget == isBudget);
            }

            return query.OrderBy(s => s.Id).ToList();
        }

        public List<string> GetGroups(IEnumerable<Student> students)
        {
            return students
                .Select(s => s.Group)
                .Where(g => g != "")
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        public int GetNextId(IEnumerable<Student> students)
        {
            if (!students.Any())
            {
                return 1;
            }
            return students.Max(s => s.Id) + 1;
        }
    }
}
