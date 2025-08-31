using LoginMonitering.DbContexts;
using LoginMonitering.DTOs;
using LoginMonitering.Models;
using LoginMonitering.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

namespace LoginMonitering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiskController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;


        public RiskController(IConfiguration config, AppDbContext context, IAuthService authService)
        {
            _config = config;
            _context = context;
            _authService = authService;
        }


        [HttpPost("verify-capcha")]
        public async Task<IActionResult> VerifyCaptcha([FromBody] CaptchaRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string ip = HttpContext.Items["RequestIp"] as string ?? "unknown";
                var res = await _authService.VerifyCaptcha(request, ip);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var riskLevels = await _context.RiskLevels.ToListAsync();

            var result = riskLevels.Select(r => new RiskLevelResDTO
            {
                Id = r.Id,
                Name = r.Name,
                MinScore = r.MinScore,
                MaxScore = r.MaxScore,
                Actions = string.IsNullOrEmpty(r.Actions)
                    ? new List<string>()
                    : r.Actions.Split(',').ToList()
            })
                .OrderBy(r => r.MinScore);

            return Ok(result);
        }




        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddRiskLevelDTO dto)
        {
            var model = new RiskLevel
            {
                Name = dto.Name,
                MinScore = dto.MinScore,
                MaxScore = dto.MaxScore,
                Actions = string.Join(",", dto.Actions)
            };

            await _context.RiskLevels.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddRiskLevelDTO dto)
        {
            var existing = await _context.RiskLevels.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.MinScore = dto.MinScore;
            existing.MaxScore = dto.MaxScore;
            existing.Actions = string.Join(",", dto.Actions);

            await _context.SaveChangesAsync();
            return Ok(existing);
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.RiskLevels.FindAsync(id);
            if (existing == null) return NotFound();

            _context.RiskLevels.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
