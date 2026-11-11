using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Infrastructure.Data;

public static class SeedData
{
    public static IReadOnlyCollection<MeetingRoom> Rooms { get; } =
    [
        new MeetingRoom { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Boardroom Alpha", Capacity = 12 },
        new MeetingRoom { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Innovation Hub", Capacity = 8 },
        new MeetingRoom { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Focus Room", Capacity = 4 }
    ];

    public static IReadOnlyCollection<Booking> Bookings { get; } =
    [
        new Booking
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Leadership Check-in",
            Start = DateTimeOffset.UtcNow.Date.AddHours(9),
            End = DateTimeOffset.UtcNow.Date.AddHours(10)
        },
        new Booking
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            RoomId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Sprint Planning",
            Start = DateTimeOffset.UtcNow.Date.AddHours(13),
            End = DateTimeOffset.UtcNow.Date.AddHours(14)
        }
    ];
}
