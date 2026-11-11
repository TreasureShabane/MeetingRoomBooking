using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IMeetingRoomRepository
{
    Task<IReadOnlyCollection<MeetingRoom>> GetAllAsync(CancellationToken cancellationToken);
    Task<MeetingRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
