using LoginMonitering.DbContexts;
using LoginMonitering.DTOs;
using LoginMonitering.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginMonitering.Services
{
    public class RiskScoringService : IRiskScoringService
    {
        private readonly AppDbContext _context;

        public RiskScoringService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RiskResult> EvaluateAsync(string ip, string userAgent, string fingerprint, Guid? userId,  string country, string userTimezone)
        {
            try
            {
                var result = new RiskResult();

                // Check blacklist
                var black = await _context.IpBlacklists.AnyAsync(b => b.IpAddress == ip);
                if (black) { result.Score += 50; result.Reasons.Add("IP in blacklist"); }

                // Check if new IP for this user (if userId provided)
                if (userId.HasValue)
                {
                    var lastLogin = await _context.LoginAttempts
                        .Where(l => l.UserId == userId && l.Result == "Success")
                        .Include(l => l.GeoLocation)
                        .OrderByDescending(l => l.AttemptedAt)
                        .FirstOrDefaultAsync();   



                    if (lastLogin == null){ 
                        result.Score += 20; 
                        result.Reasons.Add("New login(no previous successfull logins)"); 
                    }
                    else if (lastLogin.IpAddress != ip)
                    {
                        result.Score += 20;
                        result.Reasons.Add("IP changed since last login");
                    }

                    // compare last login country and current login country
                    if(country != null)
                    {
                        if (country != lastLogin?.GeoLocation?.Country)
                        {
                            result.Score += 20;
                            result.Reasons.Add("Country changed since last login");
                        }
                    }

                    // Compare fingerprint
                    if (!string.IsNullOrEmpty(lastLogin?.DeviceFingerprint) && lastLogin.DeviceFingerprint != fingerprint)
                    {
                        result.Score += 30;
                        result.Reasons.Add("Device fingerprint mismatch");
                    }

                    // Odd login time (example: outside 6am-23pm local)
                    if(userTimezone != null)
                    {
                        var timezone = TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
                        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
                        var hour = localTime.Hour;

                        if (hour < 6 || hour > 23)
                        {
                            result.Score += 10;
                            result.Reasons.Add("Odd login time");
                        }
                    }
                }
                else
                {
                    // unknown user -> slightly higher risk
                    result.Score += 10;
                    result.Reasons.Add("Unknown user (username not mapped)");
                }

                return result;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
