namespace LoginMonitering.DTOs
{
    public class AddRiskLevelDTO
    {
        public string Name { get; set; } = default!;
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public List<string> Actions { get; set; }
    }
}
