using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Interfaces;

namespace MeetingRoomBooking.Application.Services;

public sealed class MeetingRoomService(IMeetingRoomRepository meetingRoomRepository) : IMeetingRoomService
{
    public async Task<IReadOnlyCollection<MeetingRoomDto>> GetAllRoomsAsync(CancellationToken cancellationToken)
    {
        var rooms = await meetingRoomRepository.GetAllAsync(cancellationToken);
        return rooms
            .OrderBy(room => room.Name)
            .Select(room => new MeetingRoomDto(room.Id, room.Name, room.Capacity))
            .ToArray();
    }

    public async Task<MeetingRoomDto?> GetRoomAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await meetingRoomRepository.GetByIdAsync(id, cancellationToken);
        return room is null ? null : new MeetingRoomDto(room.Id, room.Name, room.Capacity);
    }
}
