namespace LoginMonitering.DTOs
{
    public class RiskLevelResDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public List<string> Actions { get; set; } = new();
    }
}
