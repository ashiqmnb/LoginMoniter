namespace LoginMonitering.DTOs
{
    public class RiskEvaluationDTO
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public string Category { get; set; } = null!;
        public string Reasons { get; set; } = null!;
        public DateTime EvaluatedAt { get; set; }
        public string? Email { get; set; } // from LoginAttempt
        public string? IpAddress { get; set; } // from LoginAttempt
    }
}
