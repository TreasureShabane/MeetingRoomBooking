using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Services;

public sealed class MeetingRoomService : IMeetingRoomService
{
    private readonly IMeetingRoomRepository _meetingRoomRepository;

    public MeetingRoomService(IMeetingRoomRepository meetingRoomRepository)
    {
        _meetingRoomRepository = meetingRoomRepository;
    }

    public IReadOnlyCollection<MeetingRoomDto> GetAllRooms()
    {
        return _meetingRoomRepository.GetAll()
            .OrderBy(room => room.Name)
            .Select(Map)
            .ToList();
    }

    public MeetingRoomDto? GetRoom(Guid id)
    {
        var room = _meetingRoomRepository.GetById(id);
        return room is null ? null : Map(room);
    }

    private static MeetingRoomDto Map(MeetingRoom room)
    {
        return new MeetingRoomDto(room.Id, room.Name, room.Capacity);
    }
}
