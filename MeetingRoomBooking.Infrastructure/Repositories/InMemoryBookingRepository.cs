using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = SeedData.Bookings.ToList();

    public Task<IReadOnlyCollection<Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<Booking>>(_bookings.AsReadOnly());
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_bookings.FirstOrDefault(booking => booking.Id == id));
    }

    public Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludedBookingId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasOverlap = _bookings.Any(booking =>
            booking.RoomId == roomId &&
            booking.Id != excludedBookingId &&
            start < booking.End &&
            booking.Start < end);

        return Task.FromResult(hasOverlap);
    }

    public Task<bool> TryCreateAsync(Booking booking, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_bookings.Any(existingBooking =>
                existingBooking.RoomId == booking.RoomId &&
                booking.Start < existingBooking.End &&
                existingBooking.Start < booking.End))
        {
            return Task.FromResult(false);
        }

        _bookings.Add(booking);
        return Task.FromResult(true);
    }

    public Task<bool> TryUpdateAsync(Booking booking, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_bookings.Any(existingBooking =>
                existingBooking.RoomId == booking.RoomId &&
                existingBooking.Id != booking.Id &&
                booking.Start < existingBooking.End &&
                existingBooking.Start < booking.End))
        {
            return Task.FromResult(false);
        }

        var index = _bookings.FindIndex(existingBooking => existingBooking.Id == booking.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Booking with id '{booking.Id}' was not found.");
        }

        _bookings[index] = booking;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_bookings.RemoveAll(booking => booking.Id == id) > 0);
    }
}
