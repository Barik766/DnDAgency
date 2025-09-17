using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnDAgency.Application.Interfaces;
using DnDAgency.Application.DTOs.CampaignsDTO;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DnDAgency.Domain.Interfaces;

namespace DnDAgency.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly IMasterRepository _masterRepository;

        public CampaignsController(ICampaignService campaignService, IMasterRepository masterRepository)
        {
            _campaignService = campaignService;
            _masterRepository = masterRepository;
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

        [HttpGet("upcoming-games")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcomingGames()
        {
            var games = await _campaignService.GetUpcomingGamesAsync();
            return Ok(games);
        }

        [HttpGet("{id}/slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSlots(Guid id)
        {
            try
            {
                var slots = await _campaignService.GetCampaignSlotsAsync(id);
                return Ok(slots);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpPost("{id}/slots")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> AddSlot(Guid id, [FromBody] CreateSlotRequest request)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                var slot = await _campaignService.AddSlotToCampaignAsync(id, request.StartTime, userId, role);
                return CreatedAtAction(nameof(GetSlots), new { id }, slot);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{campaignId}/slots/{slotId}")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> RemoveSlot(Guid campaignId, Guid slotId)
        {
            try
            {
                var role = GetCurrentUserRole();
                var userId = GetCurrentUserId();

                await _campaignService.RemoveSlotFromCampaignAsync(campaignId, slotId, userId, role);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
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

    public class CreateSlotRequest
    {
        [Required]
        public DateTime StartTime { get; set; }
    }
}
