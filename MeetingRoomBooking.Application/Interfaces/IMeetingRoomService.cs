using MeetingRoomBooking.Application.Dtos;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IMeetingRoomService
{
    Task<IReadOnlyCollection<MeetingRoomDto>> GetAllRoomsAsync(CancellationToken cancellationToken);
    Task<MeetingRoomDto?> GetRoomAsync(Guid id, CancellationToken cancellationToken);
}
