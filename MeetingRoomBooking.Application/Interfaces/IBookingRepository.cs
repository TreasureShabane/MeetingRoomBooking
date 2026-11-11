using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IBookingRepository
{
    Task<IReadOnlyCollection<Booking>> GetAllAsync(CancellationToken cancellationToken);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludedBookingId,
        CancellationToken cancellationToken);
    Task<bool> TryCreateAsync(Booking booking, CancellationToken cancellationToken);
    Task<bool> TryUpdateAsync(Booking booking, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
