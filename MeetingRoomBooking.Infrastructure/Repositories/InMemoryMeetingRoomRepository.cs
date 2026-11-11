using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class InMemoryMeetingRoomRepository : IMeetingRoomRepository
{
    private readonly List<MeetingRoom> _rooms = SeedData.Rooms.ToList();

    public Task<IReadOnlyCollection<MeetingRoom>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<MeetingRoom>>(_rooms.AsReadOnly());
    }

    public Task<MeetingRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_rooms.FirstOrDefault(room => room.Id == id));
    }
}
