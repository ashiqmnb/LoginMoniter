using Azure.Core;
using LoginMonitering.DbContexts;
using LoginMonitering.DTOs;
using LoginMonitering.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LoginMonitering.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IRiskScoringService _riskScoring;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IRiskScoringService riskScoring, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _riskScoring = riskScoring;
            _emailService = emailService;
        }


        public async Task<LoginResponseDTO> Login(string email, string password, string ip, string fingerprint, string ua)
        {
            var attempt = new LoginAttempt
            {
                Email = email,
                IpAddress = ip ?? "unknown",
                UserAgent = ua,
                DeviceFingerprint = fingerprint,
                AttemptedAt = DateTime.UtcNow,
            };

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                string? country = null;
                string? userTimezone = null;

                

                if (ip != null)
                {
                    var geo = await GetGeoLocationAsync(ip);

                    var geoRow = new GeoLocation
                    {
                        Country = geo.Country,
                        City = geo.City,
                        Query = geo.Query,
                        Timezone = geo.Timezone,
                        RegionName = geo.RegionName
                    };

                    await _context.GeoLocations.AddAsync(geoRow);
                    await _context.SaveChangesAsync();

                    int geoLocationId = geoRow.Id;

                    attempt.GeoLocationId = geoLocationId;

                    country = geoRow.Country;
                    userTimezone = geoRow.Timezone;
                }

                // return if user not exist
                if (user is null)
                {
                    attempt.Result = "Failed";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    // checking ip in black list 
                    await CheckAndBlacklistIpAsync(ip);

                    return new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "User not exist with this email"
                    };
                }

                // return if password is not match
                var isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isValidPassword)
                {
                    attempt.UserId = user.Id;
                    attempt.Result = "Failed";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    // checking IP in black list 
                    await CheckAndBlacklistIpAsync(ip);

                    return new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "Incorrect Password"
                    };
                }

                Guid? userId = user?.Id;

                var riskSettings = await _context.RiskSettingses.FirstOrDefaultAsync(rs => rs.IsActive);
                if (riskSettings == null)
                {
                    throw new Exception("No active risk settings found.");
                }

                var risk = await _riskScoring.EvaluateAsync(ip!, ua, fingerprint, userId, country, userTimezone);

                // only adding risk in DB if the score is greater than 0
                if(risk.Score > 0)
                {
                    var riskEntity = new RiskEvaluation { Score = risk.Score, Reasons = string.Join(";", risk.Reasons) };
                    await _context.RiskEvaluations.AddAsync(riskEntity);
                    await _context.SaveChangesAsync();
                    attempt.RiskEvaluationId = riskEntity.Id;
                }


                // Decision based on risk score
                if (risk.Score <= riskSettings.LowRiskMax) // Low Risk
                {
                    // allow
                    attempt.UserId = user?.Id;
                    attempt.Result = "Success";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    var token = GenerateJwtToken(user);
                    return new LoginResponseDTO
                    {
                        Status = "Success",
                        Message = "Loged In",
                        JwtToken = token,
                        Role = user.Role,
                        Email = user.Email
                    };
                }
                else if (risk.Score <= riskSettings.MediumRiskMax) // Medium Risk
                {
                    // medium: require OTP
                    attempt.UserId = user?.Id;
                    attempt.Result = "RequireOtp";
                    await _context.LoginAttempts.AddAsync(attempt);

                    // create OTP
                    var otp = new Random().Next(100000, 999999).ToString();
                    var emailOtp = new EmailOtp
                    {
                        UserId = user.Id,
                        Otp = otp,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                    };

                    var existingOtp = await _context.EmailOtps.FirstOrDefaultAsync(e => e.UserId == user.Id);
                    if(existingOtp != null)
                    {
                        _context.EmailOtps.Remove(existingOtp);
                        await _context.SaveChangesAsync();
                    }

                    await _context.EmailOtps.AddAsync(emailOtp);
                    await _context.SaveChangesAsync();

                    // send email for otp verification
                    await _emailService.SendEmail(user.Email, otp);

                    return new LoginResponseDTO
                    {
                        Status = "RequireOtp",
                        Message = "Verify Otp for login",
                        UserId = user.Id.ToString()
                    };
                }

                // High Risk
                // In high risk cases
                attempt.UserId = user?.Id;
                attempt.Result = "Blocked";
                await _context.LoginAttempts.AddAsync(attempt);
                await _context.SaveChangesAsync();

                return new LoginResponseDTO
                {
                    Status = "Failed",
                    Message = "You are blocked cause of high risk"
                };


            }
            catch (Exception ex)
            {
                attempt.Result = "Failed";
                await _context.LoginAttempts.AddAsync(attempt);
                await _context.SaveChangesAsync();

                return new LoginResponseDTO
                {
                    Status = "Failed",
                    Message = ex.Message
                };
            }
        }


        public async Task<LoginResponseDTO> VerifyOtp(VerifyOtpRequestDTO verifyOtp)
        {
            try
            {
                var emailOtp = await _context.EmailOtps
                    .FirstOrDefaultAsync(e => e.UserId.ToString() == verifyOtp.UserId && e.Otp == verifyOtp.Otp);


                if (emailOtp == null)
                {
                    var res = new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "Invalid OTP"
                    };
                    return res;
                }
                else if (emailOtp.ExpiresAt < DateTime.UtcNow)
                {
                    _context.EmailOtps.Remove(emailOtp);
                    await _context.SaveChangesAsync();

                    var res = new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "OTP has expired"
                    };
                    return res;
                }
                else
                {
                    _context.EmailOtps.Remove(emailOtp);
                    await _context.SaveChangesAsync();

                    var login = await _context.LoginAttempts
                        .Where(la => la.UserId.ToString() == verifyOtp.UserId && la.Result == "RequireOtp")
                        .OrderByDescending(la => la.AttemptedAt)
                        .FirstOrDefaultAsync();

                    login.Result = "Success";
                    await _context.SaveChangesAsync();

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == verifyOtp.UserId);

                    var token = GenerateJwtToken(user);
                    var res = new LoginResponseDTO
                    {
                        Status = "Success",
                        Message = "OTP verified successfully",
                        JwtToken = token,
                        Email = user.Email,
                        Role = user.Role
                    };
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public async Task<LoginResponseDTO> Login2(string email, string password, string ip, string fingerprint, string ua)
        {
            var attempt = new LoginAttempt
            {
                Email = email,
                IpAddress = ip ?? "unknown",
                UserAgent = ua,
                DeviceFingerprint = fingerprint,
                AttemptedAt = DateTime.UtcNow,
            };

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    attempt.Result = "Failed";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    return new LoginResponseDTO { Status = "Failed", Message = "User not found" };
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    attempt.UserId = user.Id;
                    attempt.Result = "Failed";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    return new LoginResponseDTO { Status = "Failed", Message = "Incorrect password" };
                }

                // Evaluate risk
                var risk = await _riskScoring.EvaluateAsync(ip!, ua, fingerprint, user.Id, null, null);

                // only adding risk in DB if the score is greater than 0
                if (risk.Score > 0)
                {
                    var riskEntity = new RiskEvaluation { Score = risk.Score, Reasons = string.Join(";", risk.Reasons) };
                    await _context.RiskEvaluations.AddAsync(riskEntity);
                    await _context.SaveChangesAsync();
                    attempt.RiskEvaluationId = riskEntity.Id;
                }

                var riskLevels = await _context.RiskLevels
                    .OrderBy(r => r.MinScore)
                    .ToListAsync();

                var matched = riskLevels
                    .FirstOrDefault(r => risk.Score >= r.MinScore && risk.Score <= r.MaxScore);

                if (matched == null)
                {
                    throw new Exception("No matching risk level found.");
                }

                var actions = matched.Actions.Split(',').ToList();

                var existingLoginStatus = await _context.LoginStatuses.FirstOrDefaultAsync(ls => ls.UserId == user.Id);
                if (existingLoginStatus != null)
                {
                    _context.LoginStatuses.Remove(existingLoginStatus);
                    await _context.SaveChangesAsync();
                }

                // Create login status record
                var loginStatus = new LoginStatus
                {
                    UserId = user.Id,
                    Otp = actions.Contains("Otp") ? LoginActionStatus.Required : LoginActionStatus.Null,
                    Captcha = actions.Contains("Capcha") ? LoginActionStatus.Required : LoginActionStatus.Null
                };

                //await _context.LoginStatuses.AddAsync(loginStatus);
                //await _context.SaveChangesAsync();

                // Decision flow
                if (actions.Count == 0 || actions.Contains("DirectLogin"))
                {
                    attempt.UserId = user.Id;
                    attempt.Result = "Success";
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    var token = GenerateJwtToken(user);
                    return new LoginResponseDTO
                    {
                        Status = "Success",
                        Message = "Logged In",
                        JwtToken = token,
                        Role = user.Role,
                        Email = user.Email
                    };
                }
                if (actions.Contains("Otp"))
                {
                    var otp = new Random().Next(100000, 999999).ToString();
                    var emailOtp = new EmailOtp
                    {
                        UserId = user.Id,
                        Otp = otp,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                    };

                    var existingOtp = await _context.EmailOtps.FirstOrDefaultAsync(e => e.UserId == user.Id);
                    if (existingOtp != null) _context.EmailOtps.Remove(existingOtp);

                    await _context.EmailOtps.AddAsync(emailOtp);
                    await _context.SaveChangesAsync();

                    await _emailService.SendEmail(user.Email, otp);

                    // adding login status in db for chacking completion
                    loginStatus.Otp = LoginActionStatus.Required;

                }
                if (actions.Contains("Capcha"))
                {
                    // adding login status in db for chacking completion
                    loginStatus.Captcha = LoginActionStatus.Required;
                }
                if (actions.Contains("Block"))
                {
                    attempt.Result = "Blocked";
                    attempt.UserId = user.Id;
                    await _context.LoginAttempts.AddAsync(attempt);
                    await _context.SaveChangesAsync();

                    return new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "Blocked due to high risk"
                    };
                }


                // genrerating response basen on actions
                if (actions.Contains("Capcha") || actions.Contains("Otp"))
                {
                    if(actions.Contains("Capcha") && actions.Contains("Otp"))
                    {
                        await _context.LoginStatuses.AddAsync(loginStatus);

                        attempt.Result = "RequireCapcha, RequireOtp";
                        attempt.UserId = user.Id;
                        await _context.LoginAttempts.AddAsync(attempt);

                        await _context.SaveChangesAsync();

                        return new LoginResponseDTO
                        {
                            Status = "RequireCapcha", // verify capcha first and then verfy otp
                            Message = "Please complete capcha verification and otp verification",
                            UserId = user.Id.ToString()
                        };
                    }
                    else if (actions.Contains("Capcha"))
                    {
                        await _context.LoginStatuses.AddAsync(loginStatus);

                        attempt.Result = "RequireCapcha";
                        attempt.UserId = user.Id;
                        await _context.LoginAttempts.AddAsync(attempt);

                        await _context.SaveChangesAsync();
                        return new LoginResponseDTO
                        {
                            Status = "RequireCapcha",
                            Message = "Please complete capcha verification",
                            UserId = user.Id.ToString()
                        };
                    }
                    else{
                        await _context.LoginStatuses.AddAsync(loginStatus);

                        attempt.Result = "RequireOtp";
                        attempt.UserId = user.Id;
                        await _context.LoginAttempts.AddAsync(attempt);

                        await _context.SaveChangesAsync();
                        return new LoginResponseDTO
                        {
                            Status = "RequireOtp",
                            Message = "Please complete oyp verification",
                            UserId = user.Id.ToString()
                        };
                    }
                }


                return new LoginResponseDTO();
                
            }
            catch (Exception ex)
            {
                attempt.Result = "Failed";
                await _context.LoginAttempts.AddAsync(attempt);
                await _context.SaveChangesAsync();

                return new LoginResponseDTO { Status = "Failed", Message = ex.Message };
            }
        }

        public async Task<LoginResponseDTO> VerifyOtp2(VerifyOtpRequestDTO verifyOtp, string ip)
        {
            try
            {
                var emailOtp = await _context.EmailOtps
                    .FirstOrDefaultAsync(e => e.UserId.ToString() == verifyOtp.UserId && e.Otp == verifyOtp.Otp);


                if (emailOtp == null || emailOtp.ExpiresAt < DateTime.UtcNow)
                {
                    var blackListed = await _context.IpBlacklists
                        .FirstOrDefaultAsync(i => i.IpAddress == ip);

                    if (blackListed != null && blackListed.ExpiryAt > DateTime.UtcNow)
                    {
                        return new LoginResponseDTO
                        {
                            Status = "Failed",
                            Message = "Your IP is currently blacklisted"
                        };
                    }

                    var attempt = await _context.LoginAttempts
                        .Include(la => la.RiskEvaluation)
                        .OrderByDescending(e => e.AttemptedAt)
                        .FirstOrDefaultAsync(la => la.IpAddress == ip && la.UserId.ToString() == verifyOtp.UserId);

                    if(attempt != null )
                    {
                        attempt.RiskEvaluation.Score += 10;
                        await _context.SaveChangesAsync();

                        int currentRisk = attempt.RiskEvaluation.Score;
                        if(currentRisk >= 100)
                        {
                            await AddOrUpdateBlacklistIpAsync(ip, "Blacklisted due to multiple invalid OTP attempts");
                            return new LoginResponseDTO
                            {
                                Status = "Failed",
                                Message = "Blocked due to multiple invalid OTP attempts"
                            };
                        } 
                    }

                    var res = new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "Invalid OTP or Time expired"
                    };
                    return res;
                }
                else
                {
                    var loginStatus = await _context.LoginStatuses.FirstOrDefaultAsync(ls => ls.UserId.ToString() == verifyOtp.UserId); ;
                    if (loginStatus == null)
                    {
                        return new LoginResponseDTO
                        {
                            Status = "Failed",
                            Message = "No login status found"
                        };
                    }
                    else
                    {
                        loginStatus.Otp = LoginActionStatus.Completed;
                        await _context.SaveChangesAsync();
                    }


                    var isCompleted = await CheckLoginCompleted(verifyOtp.UserId);
                    if (isCompleted)
                    {
                        var login = await _context.LoginAttempts
                        .Where(la => la.UserId.ToString() == verifyOtp.UserId && la.Result == "RequireOtp" || la.Result == "RequireCapcha, RequireOtp")
                        .OrderByDescending(la => la.AttemptedAt)
                        .FirstOrDefaultAsync();

                        login.Result = "Success";
                        _context.LoginStatuses.Remove(loginStatus);
                        await _context.SaveChangesAsync();

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == verifyOtp.UserId);

                        var token = GenerateJwtToken(user);

                        _context.EmailOtps.Remove(emailOtp);
                        await _context.SaveChangesAsync();

                        var res = new LoginResponseDTO
                        {
                            Status = "Success",
                            Message = "OTP verified successfully",
                            JwtToken = token,
                            Email = user.Email,
                            Role = user.Role
                        };
                        return res;
                    }
                    else
                    {
                        return new LoginResponseDTO
                        {
                            Status = "RequireCapcha",
                            Message = "Login not completed"
                        };
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<LoginResponseDTO> VerifyCaptcha(CaptchaRequestDTO request, string ip)
        {
            try
            {
                var secret = _config["GoogleReCaptcha:SecretKey"];


                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={request.Token}",
                    null
                );

                var json = await response.Content.ReadAsStringAsync();

                var result = System.Text.Json.JsonSerializer.Deserialize<CaptchaResponseDTO>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result != null && result.Success)
                {
                    var loginStatus = await _context.LoginStatuses.FirstOrDefaultAsync(ls => ls.UserId == request.UserId);
                    if (loginStatus == null)
                    {
                        return new LoginResponseDTO
                        {
                            Status = "Failed",
                            Message = "No login status found"
                        };
                    }
                    else
                    {
                        loginStatus.Captcha = LoginActionStatus.Completed;
                        await _context.SaveChangesAsync();
                    }

                    var isCompleted = await CheckLoginCompleted(request.UserId.ToString());
                    if (isCompleted)
                    {
                        var login = await _context.LoginAttempts
                        .Where(la => la.UserId == request.UserId && la.Result == "RequireCapcha" || la.Result == "RequireCapcha, RequireOtp")
                        .OrderByDescending(la => la.AttemptedAt)
                        .FirstOrDefaultAsync();

                        login.Result = "Success";
                        _context.LoginStatuses.Remove(loginStatus);
                        await _context.SaveChangesAsync();

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);

                        var token = GenerateJwtToken(user);
                        var res = new LoginResponseDTO
                        {
                            Status = "Success",
                            Message = "Capcha verified successfully",
                            JwtToken = token,
                            Email = user.Email,
                            Role = user.Role
                        };
                        return res;
                    }
                    else
                    {
                        return new LoginResponseDTO
                        {
                            Status = "RequireOtp",
                            Message = "Login not completed",
                            UserId = loginStatus.UserId.ToString()
                        };
                    }
                }
                else
                {
                    var blackListed = await _context.IpBlacklists
                        .FirstOrDefaultAsync(i => i.IpAddress == ip);

                    if (blackListed != null && blackListed.ExpiryAt > DateTime.UtcNow)
                    {
                        return new LoginResponseDTO
                        {
                            Status = "Failed",
                            Message = "Your IP is currently blacklisted"
                        };
                    }

                    var attempt = await _context.LoginAttempts
                        .Include(la => la.RiskEvaluation)
                        .OrderByDescending(e => e.AttemptedAt)
                        .FirstOrDefaultAsync(la => la.IpAddress == ip && la.UserId == request.UserId);

                    if (attempt != null)
                    {
                        attempt.RiskEvaluation.Score += 10;
                        await _context.SaveChangesAsync();

                        int currentRisk = attempt.RiskEvaluation.Score;
                        if (currentRisk >= 100)
                        {
                            await AddOrUpdateBlacklistIpAsync(ip, "Blacklisted due to multiple failed capcha verification");
                            return new LoginResponseDTO
                            {
                                Status = "Failed",
                                Message = "Blocked due to multiple failed capcha verification "
                            };
                        }
                    }


                    return new LoginResponseDTO
                    {
                        Status = "Failed",
                        Message = "Capcha verification failed"
                    };
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private async Task CheckAndBlacklistIpAsync(string ip)
        {
            var riskSettings = await _context.RiskSettingses.FirstOrDefaultAsync(rs => rs.IsActive);
            if (riskSettings == null)
            {
                throw new Exception("No active risk settings found.");
            }

            int timeDuration = riskSettings.TimeDuration;

            // get recent failed attempts for this IP
            var recentFailedAttempts = await _context.LoginAttempts
                .CountAsync(l => l.IpAddress == ip &&
                                 l.Result == "Failed" &&
                                 l.AttemptedAt > DateTime.UtcNow.AddMinutes(-timeDuration));

            if (recentFailedAttempts >= riskSettings.MaxFailedAttempts)
            {
                var existingBlacklist = await _context.IpBlacklists
                    .FirstOrDefaultAsync(b => b.IpAddress == ip);

                if (existingBlacklist != null)
                {
                    // check if expired
                    if (existingBlacklist.ExpiryAt <= DateTime.UtcNow)
                    {
                        // reset expiry (e.g., +4 days)
                        existingBlacklist.ExpiryAt = DateTime.UtcNow.AddDays(3);
                        existingBlacklist.Reason = "Multiple failed login attempts - expiry reset";
                        _context.IpBlacklists.Update(existingBlacklist);
                        await _context.SaveChangesAsync();
                    }
                    // else do nothing, already blacklisted
                }
                else
                {
                    // add new row
                    await _context.IpBlacklists.AddAsync(
                        new IpBlacklist
                        {
                            IpAddress = ip,
                            Reason = "Multiple failed login attempts in a small time duration",
                            ExpiryAt = DateTime.UtcNow.AddDays(3)
                        }
                    );
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task AddOrUpdateBlacklistIpAsync(string ip, string reason)
        {
            var existingIp = await _context.IpBlacklists.FirstOrDefaultAsync(b => b.IpAddress == ip);

            if (existingIp != null)
            {
                // Update existing record
                existingIp.ExpiryAt = DateTime.UtcNow.AddDays(3); // reset expiry
                existingIp.Reason = reason;
                _context.IpBlacklists.Update(existingIp);
            }
            else
            {
                // Add new record
                var newBlacklist = new IpBlacklist
                {
                    IpAddress = ip,
                    Reason = reason,
                    ExpiryAt = DateTime.UtcNow.AddDays(3)
                };

                await _context.IpBlacklists.AddAsync(newBlacklist);
            }

            await _context.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            var securityKey = _config["Jwt:SecretKey"];

            if (string.IsNullOrEmpty(securityKey))
            {
                throw new Exception("Jwt Secret key is Missing");
            }

            var key = Encoding.UTF8.GetBytes(securityKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
               {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task<GeoLocationDTO?> GetGeoLocationAsync(string ip)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://ip-api.com/json/{ip}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var geo = System.Text.Json.JsonSerializer.Deserialize<GeoLocationDTO>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return geo;
        }


        private async Task<bool> CheckLoginCompleted(string userId)
        {
            try
            {
                var loginStatus = await _context.LoginStatuses.FirstOrDefaultAsync(ls => ls.UserId.ToString() == userId);
                if (loginStatus.Otp == LoginActionStatus.Completed || loginStatus.Otp == LoginActionStatus.Null &&
                    loginStatus.Captcha == LoginActionStatus.Completed  || loginStatus.Captcha == LoginActionStatus.Null)
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
