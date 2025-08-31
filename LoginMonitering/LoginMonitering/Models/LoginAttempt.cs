namespace LoginMonitering.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; } 
        public string Email { get; set; } = null!;
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
        public string DeviceFingerprint { get; set; } = null!;
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public string Result { get; set; } = "Pending"; 
        public int? RiskEvaluationId { get; set; }

        public int? GeoLocationId { get; set; }

        public User User { get; set; }  
        public RiskEvaluation? RiskEvaluation { get; set; }
        public GeoLocation? GeoLocation { get; set; }
    }
}
