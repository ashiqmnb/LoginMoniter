namespace LoginMonitering.DTOs
{
    public class LoginResponseDTO
    {
        public string Status { get; set; }
        public string? Message { get; set; }
        public string? JwtToken { get; set; }
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }

    }
}
