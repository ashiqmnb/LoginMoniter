namespace LoginMonitering.DTOs
{
    public class CaptchaRequestDTO
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
