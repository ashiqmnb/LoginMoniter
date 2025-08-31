using LoginMonitering.DTOs;

namespace LoginMonitering.Services
{
    public interface IRiskScoringService
    {
        Task<RiskResult> EvaluateAsync(string ip, string userAgent, string fingerprint, Guid? userId, string country, string timezone);
    }
}
