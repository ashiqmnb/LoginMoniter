using LoginMonitering.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace LoginMonitering.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> Login(string email, string password, string ip, string fingerprint, string ua);
        Task<LoginResponseDTO> Login2(string email, string password, string ip, string fingerprint, string ua);
        Task<LoginResponseDTO> VerifyOtp(VerifyOtpRequestDTO verifyOtp);
        Task<LoginResponseDTO> VerifyOtp2(VerifyOtpRequestDTO verifyOtp, string ip);
        Task<LoginResponseDTO> VerifyCaptcha(CaptchaRequestDTO request, string ip);
    }
}
