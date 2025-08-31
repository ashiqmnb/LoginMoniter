namespace LoginMonitering.Models
{
    public class IpBlacklist
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime ExpiryAt { get; set; } = DateTime.UtcNow.AddDays(3);

    }
}
