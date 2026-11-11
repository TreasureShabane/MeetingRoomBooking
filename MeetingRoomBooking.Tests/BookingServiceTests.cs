using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Exceptions;
using MeetingRoomBooking.Application.Services;
using MeetingRoomBooking.Infrastructure.Repositories;

namespace MeetingRoomBooking.Tests;

public class BookingServiceTests
{
    [Fact]
    public void CreateBooking_WithValidRequest_Succeeds()
    {
        var roomRepository = new InMemoryMeetingRoomRepository();
        var bookingRepository = new InMemoryBookingRepository();
        var service = new BookingService(bookingRepository, roomRepository);

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(30);
        var request = new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Design Review",
            baseDate.AddHours(15),
            baseDate.AddHours(16));

        var booking = service.CreateBooking(request);

        Assert.Equal("Design Review", booking.Title);
        Assert.Equal(request.RoomId, booking.RoomId);
    }

    [Fact]
    public void CreateBooking_WhenRoomIsAlreadyBooked_ThrowsValidationException()
    {
        var roomRepository = new InMemoryMeetingRoomRepository();
        var bookingRepository = new InMemoryBookingRepository();
        var service = new BookingService(bookingRepository, roomRepository);

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(30);
        var firstStart = baseDate.AddHours(9);
        var firstEnd = firstStart.AddHours(1);

        service.CreateBooking(new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "First Booking",
            firstStart,
            firstEnd));

        var second = new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Conflict Booking",
            firstStart.AddMinutes(30),
            firstEnd.AddMinutes(30));

        var exception = Assert.Throws<BookingValidationException>(() => service.CreateBooking(second));

        Assert.Contains("already booked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateBooking_WhenEndBeforeStart_ThrowsValidationException()
    {
        var roomRepository = new InMemoryMeetingRoomRepository();
        var bookingRepository = new InMemoryBookingRepository();
        var service = new BookingService(bookingRepository, roomRepository);

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(30);
        var request = new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Invalid booking",
            baseDate.AddHours(10),
            baseDate.AddHours(9));

        var exception = Assert.Throws<BookingValidationException>(() => service.CreateBooking(request));

        Assert.Contains("before end time", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpdateBooking_WhenBookingBecomesInvalid_ThrowsValidationException()
    {
        var roomRepository = new InMemoryMeetingRoomRepository();
        var bookingRepository = new InMemoryBookingRepository();
        var service = new BookingService(bookingRepository, roomRepository);

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(30);

        var original = service.CreateBooking(new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Original Meeting",
            baseDate.AddHours(11),
            baseDate.AddHours(12)));

        var conflictingStart = baseDate.AddHours(13);
        var conflictingEnd = conflictingStart.AddHours(1);
        service.CreateBooking(new CreateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Conflicting Meeting",
            conflictingStart,
            conflictingEnd));

        var update = new UpdateBookingRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Original Meeting Changed",
            conflictingStart,
            conflictingEnd);

        var exception = Assert.Throws<BookingValidationException>(() => service.UpdateBooking(original.Id, update));

        Assert.Contains("already booked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeleteBooking_WhenBookingExists_RemovesBooking()
    {
        var roomRepository = new InMemoryMeetingRoomRepository();
        var bookingRepository = new InMemoryBookingRepository();
        var service = new BookingService(bookingRepository, roomRepository);

        var created = service.CreateBooking(new CreateBookingRequest(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Room cleanup",
            DateTimeOffset.UtcNow.Date.AddDays(40).AddHours(14),
            DateTimeOffset.UtcNow.Date.AddDays(40).AddHours(15)));

        service.DeleteBooking(created.Id);

        Assert.Null(service.GetBooking(created.Id));
    }
}
