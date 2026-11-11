using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class InMemoryMeetingRoomRepository : IMeetingRoomRepository
{
    private readonly List<MeetingRoom> _rooms;

    public InMemoryMeetingRoomRepository()
    {
        _rooms = SeedData.Rooms.ToList();
    }

    public IReadOnlyCollection<MeetingRoom> GetAll()
    {
        return _rooms.AsReadOnly();
    }

    public MeetingRoom? GetById(Guid id)
    {
        return _rooms.FirstOrDefault(room => room.Id == id);
    }
}
