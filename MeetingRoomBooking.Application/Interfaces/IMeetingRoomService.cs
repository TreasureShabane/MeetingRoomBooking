using MeetingRoomBooking.Application.Dtos;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IMeetingRoomService
{
    IReadOnlyCollection<MeetingRoomDto> GetAllRooms();
    MeetingRoomDto? GetRoom(Guid id);
}
