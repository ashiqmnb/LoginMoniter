namespace LoginMonitering.DTOs
{
    public class RiskSettingsDTO
    {
        public int LowRiskMax { get; set; }
        public int MediumRiskMax { get; set; }
        public int MaxFailedAttempts { get; set; }
        public int TimeDuration { get; set; }
    }
}
