namespace LoginMonitering.Models
{
    public class RiskLevel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public string Actions { get; set; } = "[]";
    }
}
