namespace MeetingRoomBooking.Application.Dtos;

/// <summary>Fields required to create a meeting room booking.</summary>
public sealed record CreateBookingRequest(
    Guid RoomId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
