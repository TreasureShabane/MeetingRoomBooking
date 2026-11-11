using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>Lists all bookings, optionally filtered by meeting room.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> GetAll(
        [FromQuery] Guid? roomId,
        CancellationToken cancellationToken)
    {
        var bookings = roomId is null
            ? await bookingService.GetAllBookingsAsync(cancellationToken)
            : await bookingService.GetBookingsForRoomAsync(roomId.Value, cancellationToken);

        return Ok(bookings);
    }

    /// <summary>Returns a booking by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetBookingAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Creates a booking when the room is available for the requested time.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var created = await bookingService.CreateBookingAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a booking after validating the requested room and time.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Update(
        Guid id,
        [FromBody] UpdateBookingRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await bookingService.UpdateBookingAsync(id, request, cancellationToken));
    }

    /// <summary>Cancels a booking by deleting it.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await bookingService.DeleteBookingAsync(id, cancellationToken);
        return NoContent();
    }
}
