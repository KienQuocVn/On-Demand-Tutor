using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.ModelViews.Booking;
using System.Threading.Tasks;

namespace OnDemandTutor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST api/booking/book-subject/slot  
        [HttpPost("book-subject/slot")]
        public async Task<IActionResult> BookSubjectBySlot([FromBody] SlotBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookingService.BookSubjectBySlot(dto);
            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(result);
        }

        // POST api/booking/book-subject/time  
        [HttpPost("book-subject/time")]
        public async Task<IActionResult> BookSubjectByTime([FromBody] TimeBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookingService.BookSubjectByTime(dto);
            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(result);
        }
    }
}
