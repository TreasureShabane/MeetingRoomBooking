using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class SqliteBookingRepository(BookingDbContext dbContext) : IBookingRepository
{
    public async Task<IReadOnlyCollection<Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    public Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludedBookingId,
        CancellationToken cancellationToken)
    {
        var overlappingBookings = dbContext.Bookings.Where(
            booking => booking.RoomId == roomId && start < booking.End && booking.Start < end);
        if (excludedBookingId is Guid currentBookingId)
        {
            overlappingBookings = overlappingBookings.Where(booking => booking.Id != currentBookingId);
        }

        return overlappingBookings.AnyAsync(cancellationToken);
    }

    public async Task<bool> TryCreateAsync(Booking booking, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        if (await HasOverlapAsync(
                booking.RoomId,
                booking.Start,
                booking.End,
                null,
                cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TryUpdateAsync(Booking booking, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        if (await HasOverlapAsync(
                booking.RoomId,
                booking.Start,
                booking.End,
                booking.Id,
                cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        dbContext.Bookings.Update(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
        {
            return false;
        }

        dbContext.Bookings.Remove(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
