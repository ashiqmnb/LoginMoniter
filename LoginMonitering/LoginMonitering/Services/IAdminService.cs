using LoginMonitering.DTOs;
using LoginMonitering.Models;

namespace LoginMonitering.Services
{
    public interface IAdminService
    {
        Task<List<LoginAttemptDTO>> GetLoginAttepts(string result);
        Task<List<UserLoginStatsDTO>> GetUserLoginStats();
        Task<List<RiskEvaluationDTO>> GetRiskEvaluations(string? riskLevel = null);
        Task<List<GeoLocationDTO>> GetAllDistinctLocations();
        Task<List<IpBlacklist>> GetBlackList();
        Task<bool> AddRiskSettings(RiskSettingsDTO riskSettings);
        Task<bool> SwitchToActive(int id);
        Task<List<RiskSettings>> GetAllRiskSttings();
        Task<bool> DeleteSettings(int id);
        Task<bool> UpdateRiskSettings(int id, RiskSettingsDTO riskSettings);
    }
}
