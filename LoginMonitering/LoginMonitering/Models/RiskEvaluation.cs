namespace LoginMonitering.Models
{
    public class RiskEvaluation
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public string Reasons { get; set; } = null!; 
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<LoginAttempt> LoginAttempts { get; set; }
    }
}
