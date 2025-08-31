namespace LoginMonitering.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string receiverMail, string otp);
    }
}
