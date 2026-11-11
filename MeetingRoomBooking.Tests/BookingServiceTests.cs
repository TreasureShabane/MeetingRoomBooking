using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Exceptions;
using MeetingRoomBooking.Application.Services;
using MeetingRoomBooking.Infrastructure.Repositories;

namespace MeetingRoomBooking.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBooking_WithValidRequest_Succeeds()
    {
        var service = CreateService();
        var request = new CreateBookingRequest(
            RoomId,
            "Design Review",
            FutureDate.AddHours(15),
            FutureDate.AddHours(16));

        var booking = await service.CreateBookingAsync(request, CancellationToken.None);

        Assert.Equal("Design Review", booking.Title);
        Assert.Equal(request.RoomId, booking.RoomId);
    }

    [Fact]
    public async Task CreateBooking_WhenRoomIsAlreadyBooked_ThrowsConflictException()
    {
        var service = CreateService();
        var start = FutureDate.AddHours(9);
        var end = start.AddHours(1);

        await service.CreateBookingAsync(
            new CreateBookingRequest(RoomId, "First Booking", start, end),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BookingConflictException>(() =>
            service.CreateBookingAsync(
                new CreateBookingRequest(RoomId, "Conflict Booking", start.AddMinutes(30), end.AddMinutes(30)),
                CancellationToken.None));

        Assert.Contains("already booked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBooking_WhenEndBeforeStart_ThrowsValidationException()
    {
        var service = CreateService();
        var request = new CreateBookingRequest(
            RoomId,
            "Invalid booking",
            FutureDate.AddHours(10),
            FutureDate.AddHours(9));

        var exception = await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.CreateBookingAsync(request, CancellationToken.None));

        Assert.Contains("before end time", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateBooking_WhenBookingBecomesInvalid_ThrowsConflictException()
    {
        var service = CreateService();
        var original = await service.CreateBookingAsync(
            new CreateBookingRequest(RoomId, "Original Meeting", FutureDate.AddHours(11), FutureDate.AddHours(12)),
            CancellationToken.None);
        var conflictingStart = FutureDate.AddHours(13);
        var conflictingEnd = conflictingStart.AddHours(1);

        await service.CreateBookingAsync(
            new CreateBookingRequest(RoomId, "Conflicting Meeting", conflictingStart, conflictingEnd),
            CancellationToken.None);

        var update = new UpdateBookingRequest(RoomId, "Original Meeting Changed", conflictingStart, conflictingEnd);
        var exception = await Assert.ThrowsAsync<BookingConflictException>(() =>
            service.UpdateBookingAsync(original.Id, update, CancellationToken.None));

        Assert.Contains("already booked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateBooking_WithValidChanges_UpdatesTheExistingBooking()
    {
        var service = CreateService();
        var original = await service.CreateBookingAsync(
            new CreateBookingRequest(RoomId, "Original Meeting", FutureDate.AddHours(11), FutureDate.AddHours(12)),
            CancellationToken.None);
        var updatedStart = FutureDate.AddHours(14);
        var updatedEnd = updatedStart.AddHours(1);

        var updated = await service.UpdateBookingAsync(
            original.Id,
            new UpdateBookingRequest(SecondRoomId, "Updated Meeting", updatedStart, updatedEnd),
            CancellationToken.None);

        Assert.Equal(original.Id, updated.Id);
        Assert.Equal(SecondRoomId, updated.RoomId);
        Assert.Equal("Updated Meeting", updated.Title);
        Assert.Equal(updatedStart, updated.Start);
        Assert.Equal(updatedEnd, updated.End);
    }

    [Fact]
    public async Task DeleteBooking_WhenBookingExists_RemovesBooking()
    {
        var service = CreateService();
        var created = await service.CreateBookingAsync(
            new CreateBookingRequest(
                SecondRoomId,
                "Room cleanup",
                FutureDate.AddHours(14),
                FutureDate.AddHours(15)),
            CancellationToken.None);

        await service.DeleteBookingAsync(created.Id, CancellationToken.None);

        Assert.Null(await service.GetBookingAsync(created.Id, CancellationToken.None));
    }

    private static BookingService CreateService()
    {
        return new BookingService(new InMemoryBookingRepository(), new InMemoryMeetingRoomRepository());
    }

    private static readonly Guid RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondRoomId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset FutureDate = DateTimeOffset.UtcNow.Date.AddDays(30);
}
