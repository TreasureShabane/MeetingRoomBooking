namespace MeetingRoomBooking.Application.Dtos;

public sealed record BookingDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
