namespace LoginMonitering.DTOs
{
    public class RiskResult
    {
        public int Score { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }
}
