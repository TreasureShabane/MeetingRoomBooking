namespace MeetingRoomBooking.Application.Dtos;

/// <summary>Fields required to update a meeting room booking.</summary>
public sealed record UpdateBookingRequest(
    Guid RoomId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
