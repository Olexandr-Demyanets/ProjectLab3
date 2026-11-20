namespace ProjectLab3.Models
{
    public class Discipline
    {
        public string Name { get; set; } = "";
        public int Grade { get; set; }

        public Discipline()
        {
        }

        public Discipline(string name, int grade)
        {
            Name = name;
            Grade = grade;
        }
    }
}
