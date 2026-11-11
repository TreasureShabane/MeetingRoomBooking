namespace MeetingRoomBooking.Application.Dtos;

public sealed record UpdateBookingRequest(
    Guid RoomId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
