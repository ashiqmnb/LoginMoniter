using LoginMonitering.DTOs;
using LoginMonitering.Models;
using LoginMonitering.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoginMonitering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string ip = HttpContext.Items["RequestIp"] as string ?? "unknown";
                var fingerprint = loginDto.Fingerprint ?? (HttpContext.Items["DeviceFingerprint"] as string ?? "");
                var ua = HttpContext.Items["UserAgent"] as string ?? Request.Headers["User-Agent"].ToString();

                var res = await _authService.Login2(loginDto.Email, loginDto.Password, ip, fingerprint, ua);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPatch("verifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody]VerifyOtpRequestDTO verifyOtpDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string ip = HttpContext.Items["RequestIp"] as string ?? "unknown";
                var res = await _authService.VerifyOtp2(verifyOtpDto, ip);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpGet("Geo/{ip}")]
        public async Task<IActionResult> GetGeoLocation(string ip)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://ip-api.com/json/{ip}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var geo = System.Text.Json.JsonSerializer.Deserialize<GeoLocationDTO>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return Ok(geo);
        }
    }
}
