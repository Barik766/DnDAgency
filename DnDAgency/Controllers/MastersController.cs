using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnDAgency.Application.Interfaces;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using DnDAgency.Application.DTOs.MastersDTO;
using Microsoft.AspNetCore.Hosting;

namespace DnDAgency.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MastersController : ControllerBase
    {
        private readonly IMasterService _masterService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MastersController(IMasterService masterService, IWebHostEnvironment webHostEnvironment)
        {
            _masterService = masterService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var masters = await _masterService.GetAllAsync();
            return Ok(masters);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var master = await _masterService.GetByIdAsync(id);
                return Ok(master);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("admin-create")]
        public async Task<IActionResult> AdminCreate([FromForm] AdminCreateMasterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Bio))
                return BadRequest(new { message = "Name and Bio are required" });

            try
            {
                var master = await _masterService.AdminCreateMasterAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = master.Id }, master);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create-profile")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateMasterProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var master = await _masterService.CreateFromUserAsync(userId, request.Bio);
                return CreatedAtAction(nameof(GetById), new { id = master.Id }, master);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMasterDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var master = await _masterService.UpdateAsync(id, request, currentUserId);
                return Ok(master);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _masterService.DeactivateAsync(id, currentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{id}/campaigns")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCampaigns(Guid id)
        {
            try
            {
                var campaigns = await _masterService.GetMasterCampaignsAsync(id);
                return Ok(campaigns);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{masterId}/campaigns")]
        public async Task<IActionResult> AddCampaignToMaster(Guid masterId, [FromBody] AddCampaignToMasterDto dto)
        {
            await _masterService.AddCampaignToMasterAsync(masterId, dto.CampaignId);
            return Ok(new { message = "Campaign added to master successfully" });
        }

        [HttpPost("{id}/campaigns/assign")]
        [Authorize(Roles = "Master,Admin")]
        public async Task<IActionResult> AssignCampaigns(Guid id, [FromBody] AssignCampaignsDto dto)
        {
            var currentUserId = GetCurrentUserId();

            await _masterService.AssignCampaignsAsync(id, dto.CampaignIds, currentUserId);

            return Ok(new { Message = "Campaigns assigned successfully" });
        }

        [HttpGet("{id}.{ext}")]
        [AllowAnonymous]
        public IActionResult GetImage(Guid id, string ext)
        {
            var allowedExt = new[] { "jpg", "jpeg", "png" };
            ext = ext.ToLowerInvariant();

            if (!allowedExt.Contains(ext))
                return BadRequest("Unsupported image extension.");

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "masters", $"{id}.{ext}");
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var mimeType = ext switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, mimeType);
        }


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token");
            return userId;
        }
    }

    public class CreateMasterProfileRequest
    {
        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Bio must be between 10 and 2000 characters")]
        public string Bio { get; set; } = string.Empty;
    }
}