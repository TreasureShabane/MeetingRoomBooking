using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class SqliteMeetingRoomRepository(BookingDbContext dbContext) : IMeetingRoomRepository
{
    public async Task<IReadOnlyCollection<MeetingRoom>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.MeetingRooms
            .AsNoTracking()
            .OrderBy(room => room.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<MeetingRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.MeetingRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.Id == id, cancellationToken);
    }
}
