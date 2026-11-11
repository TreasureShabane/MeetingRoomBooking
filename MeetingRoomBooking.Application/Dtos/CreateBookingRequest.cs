namespace MeetingRoomBooking.Application.Dtos;

public sealed record CreateBookingRequest(
    Guid RoomId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
