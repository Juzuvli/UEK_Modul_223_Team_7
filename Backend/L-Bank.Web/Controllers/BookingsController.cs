using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank_W_Backend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository bookingRepository;

        public BookingsController(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Post([FromBody] Booking booking)
        {
            if (booking == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid booking data.");
            }

            try
            {
                bool success = await Task.Run(() =>
                    bookingRepository.Book(booking.SourceId, booking.DestinationId, booking.Amount));

                return success ? Ok("Booking successful.") : Conflict("Insufficient funds or transaction failed.");
            }
            catch (Exception ex)
            {
                return Conflict($"An error occurred: {ex.Message}");
            }
        }
    }
}
