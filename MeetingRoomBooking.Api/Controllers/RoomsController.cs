using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RoomsController : ControllerBase
{
    private readonly IMeetingRoomService _meetingRoomService;

    public RoomsController(IMeetingRoomService meetingRoomService)
    {
        _meetingRoomService = meetingRoomService;
    }

    /// <summary>Lists the meeting rooms available for booking.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<MeetingRoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<MeetingRoomDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _meetingRoomService.GetAllRoomsAsync(cancellationToken));
    }

    /// <summary>Returns a meeting room by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MeetingRoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingRoomDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _meetingRoomService.GetRoomAsync(id, cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }
}
