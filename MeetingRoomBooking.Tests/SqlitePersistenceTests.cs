using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;
using MeetingRoomBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBooking.Tests;

public class SqlitePersistenceTests
{
    [Fact]
    public async Task BookingRepository_PersistsBookingsAcrossDbContexts()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Persistence check",
            Start = DateTimeOffset.UtcNow.Date.AddDays(30).AddHours(9),
            End = DateTimeOffset.UtcNow.Date.AddDays(30).AddHours(10)
        };

        try
        {
            await using (var context = new BookingDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                var repository = new SqliteBookingRepository(context);
                Assert.True(await repository.TryCreateAsync(booking, CancellationToken.None));
                Assert.True(await repository.HasOverlapAsync(
                    booking.RoomId,
                    booking.Start.AddMinutes(30),
                    booking.End.AddMinutes(30),
                    null,
                    CancellationToken.None));
                Assert.False(await repository.HasOverlapAsync(
                    booking.RoomId,
                    booking.Start,
                    booking.End,
                    booking.Id,
                    CancellationToken.None));
            }

            await using (var context = new BookingDbContext(options))
            {
                var persisted = await new SqliteBookingRepository(context)
                    .GetByIdAsync(booking.Id, CancellationToken.None);

                Assert.NotNull(persisted);
                Assert.Equal(booking.Title, persisted.Title);
                Assert.Equal(booking.Start, persisted.Start);
                Assert.Equal(booking.End, persisted.End);
            }
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task BookingRepository_ConcurrentOverlappingCreates_OnlyOneSucceeds()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .Options;
        var start = DateTimeOffset.UtcNow.Date.AddDays(30).AddHours(9);
        var bookings = new[]
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Concurrent booking one",
                Start = start,
                End = start.AddHours(1)
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Concurrent booking two",
                Start = start.AddMinutes(30),
                End = start.AddHours(2)
            }
        };

        try
        {
            await using (var context = new BookingDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();
            }

            await using var firstContext = new BookingDbContext(options);
            await using var secondContext = new BookingDbContext(options);
            var firstCreate = new SqliteBookingRepository(firstContext)
                .TryCreateAsync(bookings[0], CancellationToken.None);
            var secondCreate = new SqliteBookingRepository(secondContext)
                .TryCreateAsync(bookings[1], CancellationToken.None);
            var results = await Task.WhenAll(firstCreate, secondCreate);

            Assert.Single(results, result => result);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
