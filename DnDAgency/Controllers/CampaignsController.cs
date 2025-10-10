using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnDAgency.Application.Interfaces;
using DnDAgency.Application.DTOs.CampaignsDTO;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DnDAgency.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace DnDAgency.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly IMasterRepository _masterRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CampaignsController(ICampaignService campaignService, IMasterRepository masterRepository, IWebHostEnvironment webHostEnvironment)
        {
            _campaignService = campaignService;
            _masterRepository = masterRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // ---------------- PUBLIC ----------------

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var campaigns = await _campaignService.GetAllAsync();
            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var campaign = await _campaignService.GetCampaignDetailsAsync(id);
                return Ok(campaign);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("catalog")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCatalog()
        {
            var catalog = await _campaignService.GetCampaignCatalogAsync();
            return Ok(catalog);
        }

        [HttpGet("catalog/paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCatalogPaged(
        [FromQuery] string? search,
        [FromQuery] string? tag,
        [FromQuery] bool? hasSlots,
        [FromQuery] string sortBy = "title",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
        {
            var filter = new CampaignFilterDto
            {
                Search = search,
                Tag = tag,
                HasSlots = hasSlots,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _campaignService.GetCampaignCatalogFilteredAsync(filter);
            return Ok(result);
        }

        [HttpGet("upcoming-games")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcomingGames()
        {
            var games = await _campaignService.GetUpcomingGamesAsync();
            return Ok(games);
        }

        [HttpGet("{campaignId}/available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTimeSlots(Guid campaignId, [FromQuery] DateTime date, [FromQuery] string roomType)
        {
            try
            {
                if (!Enum.TryParse<RoomType>(roomType, true, out var roomTypeEnum))
                {
                    return BadRequest("Invalid room type. Use 'Online' or 'Physical'");
                }

                var slots = await _campaignService.GetAvailableTimeSlotsAsync(campaignId, date, roomTypeEnum);
                return Ok(new { Success = true, Data = slots, Message = "Request completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id}.jpg")]
        [AllowAnonymous]
        private IActionResult GetImage(Guid id)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "campaigns", $"{id}.jpg");
                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var fileStream = System.IO.File.OpenRead(filePath);
                return File(fileStream, "image/jpeg");
            }
            else
            {
                var bucketName = "dnd-agency-images";
                var key = $"campaigns/{id}.jpg";
                var url = $"https://{bucketName}.s3.eu-north-1.amazonaws.com/{key}";
                return Redirect(url);
            }
        }

        // ---------------- PRIVATE / MASTER / ADMIN ----------------

        [HttpPost]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> Create([FromForm] CreateCampaignDto request)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                var campaign = await _campaignService.CreateAsync(request, userId, role);
                return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, campaign);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCampaignDto request)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                var campaign = await _campaignService.UpdateAsync(id, request, userId, role);
                return Ok(campaign);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                await _campaignService.DeleteAsync(id, userId, role);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                var campaign = await _campaignService.ToggleStatusAsync(id, userId, role);
                return Ok(campaign);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        // ---------------- HELPERS ----------------

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token");
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Master";
        }
    }
}