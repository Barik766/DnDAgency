using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Exceptions;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using DnDAgency.Application.DTOs.BookingsDTO;

namespace DnDAgency.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.CreateBookingAsync(userId, request.SlotId);
                return CreatedAtAction(nameof(GetUserBookings), new { }, booking);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (PastSlotBookingException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (SlotFullException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = GetCurrentUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBooking(Guid bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);
                var booking = bookings.FirstOrDefault(b => b.Id == bookingId);

                if (booking == null)
                    return NotFound(new { message = "Booking not found or access denied" });

                return Ok(booking);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _bookingService.CancelBookingAsync(bookingId, userId);
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("slots/{campaignId}/available")]
        [AllowAnonymous] // Доступные слоты может посмотреть любой
        public async Task<IActionResult> GetAvailableSlots(Guid campaignId)
        {
            try
            {
                var slots = await _bookingService.GetAvailableSlotsAsync(campaignId);
                return Ok(slots);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token");
            return userId;
        }
    }
}