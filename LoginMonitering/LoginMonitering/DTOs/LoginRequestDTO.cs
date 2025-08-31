namespace LoginMonitering.DTOs
{
    public class LoginRequestDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Fingerprint { get; set; }
    }
}
