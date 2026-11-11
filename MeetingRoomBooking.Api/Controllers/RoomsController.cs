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

    [HttpGet]
    public ActionResult<IReadOnlyCollection<MeetingRoomDto>> GetAll()
    {
        return Ok(_meetingRoomService.GetAllRooms());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<MeetingRoomDto> GetById(Guid id)
    {
        var room = _meetingRoomService.GetRoom(id);
        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }
}
