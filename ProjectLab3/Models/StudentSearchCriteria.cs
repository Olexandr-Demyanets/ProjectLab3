namespace ProjectLab3.Models
{
    public class StudentSearchCriteria
    {
        public string NamePart { get; set; } = "";
        public string? Group { get; set; }
        public double? MinAverage { get; set; }
        public bool? IsBudget { get; set; }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(NamePart)
                    && Group == null
                    && MinAverage == null
                    && IsBudget == null;
            }
        }
    }
}
