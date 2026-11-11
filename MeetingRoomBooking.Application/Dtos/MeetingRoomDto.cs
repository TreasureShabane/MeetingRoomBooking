namespace MeetingRoomBooking.Application.Dtos;

/// <summary>Details of a meeting room available for booking.</summary>
public sealed record MeetingRoomDto(Guid Id, string Name, int Capacity);
