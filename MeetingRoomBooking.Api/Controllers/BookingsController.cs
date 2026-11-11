using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Exceptions;
using MeetingRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<BookingDto>> GetAll([FromQuery] Guid? roomId)
    {
        var bookings = roomId is null
            ? _bookingService.GetAllBookings()
            : _bookingService.GetBookingsForRoom(roomId.Value);

        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<BookingDto> GetById(Guid id)
    {
        var booking = _bookingService.GetBooking(id);
        if (booking is null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPost]
    public ActionResult<BookingDto> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var created = _bookingService.CreateBooking(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (BookingValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more validation errors occurred.",
                errors = ex.Errors
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public ActionResult<BookingDto> Update(Guid id, [FromBody] UpdateBookingRequest request)
    {
        try
        {
            var updated = _bookingService.UpdateBooking(id, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BookingValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more validation errors occurred.",
                errors = ex.Errors
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            _bookingService.DeleteBooking(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
