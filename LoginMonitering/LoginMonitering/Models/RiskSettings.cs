namespace LoginMonitering.Models
{
    public class RiskSettings
    {
        public int Id { get; set; }
        public int LowRiskMax { get; set; }
        public int MediumRiskMax { get; set; }
        public int MaxFailedAttempts { get; set; }
        public int TimeDuration { get; set; }
        public bool IsActive { get; set; }
    }
}