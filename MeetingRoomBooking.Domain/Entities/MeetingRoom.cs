namespace MeetingRoomBooking.Domain.Entities;

public class MeetingRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public int Capacity { get; set; }
}
