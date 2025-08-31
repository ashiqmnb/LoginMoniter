using LoginMonitering.DbContexts;
using LoginMonitering.DTOs;
using LoginMonitering.Migrations;
using LoginMonitering.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace LoginMonitering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }



        [HttpGet("GetAllLoginAttempts")]
        public async Task<IActionResult> GetLoginAttempts([FromQuery] string? result)
        {
            try
            {
                var res = await _adminService.GetLoginAttepts(result);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("login-stats")]
        public async Task<IActionResult> GetUserLoginStats()
        {
            try
            {
                var res = await _adminService.GetUserLoginStats();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("risk-evaluations")]
        public async Task<IActionResult> GetRiskEvaluations([FromQuery] string? riskLevel)
        {
            try
            {
                var res = await _adminService.GetRiskEvaluations(riskLevel);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("all-locations")]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var locations = await _adminService.GetAllDistinctLocations();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("blacklist")]
        public async Task<IActionResult> GetBlackList()
        {
            try
            {
                var blackList = await _adminService.GetBlackList();
                return Ok(blackList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }





        [HttpPost("risk-settings")]
        public async Task<IActionResult> AddRiskSettings([FromBody] RiskSettingsDTO riskSettings)
        {
            try
            {
                await _adminService.AddRiskSettings(riskSettings);
                return Ok(new
                {
                    Status = "Success",
                    Message = "Risk settings added successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Failed",
                    Message = ex.Message
                });
            }
        }



        [HttpGet("get-risk-settings")]
        public async Task<IActionResult> GetAllRiskSttings()
        {
            try
            {
                var res = await _adminService.GetAllRiskSttings();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPatch("switch-active/{id}")]
        public async Task<IActionResult> SwitchToActive(int id)
        {
            try
            {
                var res = await _adminService.SwitchToActive(id);
                return Ok("Active risk settings changed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }




        [HttpDelete("delete-risk-settings/{id}")]
        public async Task<IActionResult> DeleteSettings(int id)
        {
            try
            {
                var res = await _adminService.DeleteSettings(id);
                return Ok("Risk settings deleted successfull");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPut("risk-settings/{id}")]
        public async Task<IActionResult> UpdateRiskSettings(int id, [FromBody] RiskSettingsDTO riskSettings)
        {
            try
            {
                var res = await _adminService.UpdateRiskSettings(id, riskSettings);

                return Ok(new
                {
                    Status = "Success",
                    Message = "Risk settings updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Failed",
                    Message = ex.Message
                });
            }
        }

    }
}
