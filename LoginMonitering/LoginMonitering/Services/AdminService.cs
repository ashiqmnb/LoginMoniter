using Azure.Core;
using LoginMonitering.DbContexts;
using LoginMonitering.DTOs;
using LoginMonitering.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginMonitering.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RiskEvaluationDTO>> GetRiskEvaluations(string? riskLevel = null)
        {
            
            var evaluations = await _context.RiskEvaluations
                .Include(r => r.LoginAttempts)
                .OrderByDescending(r => r.EvaluatedAt)
                .ToListAsync();

            var result = evaluations
                .SelectMany(re => re.LoginAttempts.DefaultIfEmpty(), (re, la) => new RiskEvaluationDTO
                {
                    Id = re.Id,
                    Score = re.Score,
                    Category = re.Score <= 30 ? "Low Risk" : re.Score <= 60 ? "Medium Risk" : "High Risk",
                    Reasons = re.Reasons,
                    EvaluatedAt = re.EvaluatedAt,
                    Email = la?.Email,
                    IpAddress = la?.IpAddress
                })
                .Where(x => riskLevel == null || x.Category.Equals(riskLevel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return result;
        }


        public async Task<List<LoginAttemptDTO>> GetLoginAttepts(string? result)
        {
            try
            {
                var attempts = await _context.LoginAttempts
                    .Select(a => new LoginAttemptDTO
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        Email = a.Email,
                        IpAddress = a.IpAddress,
                        UserAgent = a.UserAgent,
                        DeviceFingerprint = a.DeviceFingerprint,
                        AttemptedAt = a.AttemptedAt,
                        Result = a.Result
                    })
                    .Where(a => a.Email != "admin@gmail.com")
                    .OrderByDescending(a => a.AttemptedAt)
                    .ToListAsync();

                if (string.IsNullOrEmpty(result) || result == "all")
                {
                    return attempts;
                }

                string filter = result.ToLower();

                return attempts
                    .Where(a => a.Result.ToLower() != null && a.Result.ToLower() == filter)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }


        public async Task<List<UserLoginStatsDTO>> GetUserLoginStats()
        {
            try
            {
                var result = await _context.Users
                .Select(u => new UserLoginStatsDTO
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    SuccessCount = u.LoginAttempts.Count(a => a.Result == "Success"),
                    FailedCount = u.LoginAttempts.Count(a => a.Result == "Failed"),
                    BlockedCount = u.LoginAttempts.Count(a => a.Result == "Blocked")
                })
                .Where(a => a.Email != "admin@gmail.com")
                .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }


        public async Task<List<GeoLocationDTO>> GetAllDistinctLocations()
        {
            try
            {
                var locations = await _context.GeoLocations
                    .GroupBy(g => new { g.Country, g.RegionName, g.City, g.Timezone, g.Query })
                    .Select(g => new GeoLocationDTO
                    {
                        Country = g.Key.Country,
                        RegionName = g.Key.RegionName,
                        City = g.Key.City,
                        Timezone = g.Key.Timezone,
                        Query = g.Key.Query
                    })
                    .ToListAsync();

                return locations;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }


        public async Task<List<IpBlacklist>> GetBlackList()
        {
            try
            {
                var blackList = await _context.IpBlacklists.ToListAsync();
                return blackList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }


        public async Task<bool> AddRiskSettings(RiskSettingsDTO riskSettings)
        {
            try
            {
                if(riskSettings == null)
                {
                    throw new Exception("Risk setting cannot be null");
                }

                if(riskSettings.LowRiskMax <= 0 || riskSettings.MediumRiskMax <= 0 || riskSettings.TimeDuration <= 0 || riskSettings.MaxFailedAttempts <=0)
                {
                    throw new Exception("All values must be greater than 0");
                }

                if (riskSettings.MediumRiskMax <= riskSettings.LowRiskMax)
                {
                    throw new Exception("MediumRiskMax must be greater than LowRiskMax.");
                }
                var riskSettigsRow = new RiskSettings
                {
                    IsActive = false,
                    LowRiskMax = riskSettings.LowRiskMax,
                    MediumRiskMax = riskSettings.MediumRiskMax,
                    MaxFailedAttempts = riskSettings.MaxFailedAttempts,
                    TimeDuration = riskSettings.TimeDuration
                };

                await _context.RiskSettingses.AddAsync(riskSettigsRow);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<bool> SwitchToActive(int id)
        {
            try
            {
                if (id == null) throw new Exception("Risk settings id cannot be null");

                var active = await _context.RiskSettingses.FirstOrDefaultAsync(r => r.IsActive == true);
                if (active != null)
                {
                    active.IsActive = false;
                }
                
                var newActive = await _context.RiskSettingses.FirstOrDefaultAsync(r => r.Id == id);

                if (newActive == null) throw new Exception("No risk settings is find in this id");

                newActive.IsActive = true;
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<RiskSettings>> GetAllRiskSttings()
        {
            try
            {
                return await _context.RiskSettingses.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }


        public async Task<bool> DeleteSettings(int id)
        {
            try
            {
                var risk = await _context.RiskSettingses.FirstOrDefaultAsync(r => r.Id == id);
                _context.RiskSettingses.Remove(risk);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching login attempts", ex);
            }
        }

        public async Task<bool> UpdateRiskSettings(int id, RiskSettingsDTO riskSettings)
        {
            try
            {
                if (riskSettings == null)
                {
                    throw new Exception("Risk setting cannot be null");
                }

                if (riskSettings.LowRiskMax <= 0 || riskSettings.MediumRiskMax <= 0
                    || riskSettings.TimeDuration <= 0 || riskSettings.MaxFailedAttempts <= 0)
                {
                    throw new Exception("All values must be greater than 0");
                }

                if (riskSettings.MediumRiskMax <= riskSettings.LowRiskMax)
                {
                    throw new Exception("MediumRiskMax must be greater than LowRiskMax.");
                }

                var existingRiskSetting = await _context.RiskSettingses.FindAsync(id);
                if (existingRiskSetting == null)
                {
                    throw new Exception("Risk settings cannot find with this id");
                }

                existingRiskSetting.LowRiskMax = riskSettings.LowRiskMax;
                existingRiskSetting.MediumRiskMax = riskSettings.MediumRiskMax;
                existingRiskSetting.MaxFailedAttempts = riskSettings.MaxFailedAttempts;
                existingRiskSetting.TimeDuration = riskSettings.TimeDuration;
                
                _context.RiskSettingses.Update(existingRiskSetting);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
