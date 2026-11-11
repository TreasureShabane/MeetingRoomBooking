namespace MeetingRoomBooking.Application.Dtos;

/// <summary>Booking details returned by the meeting room API.</summary>
public sealed record BookingDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);
