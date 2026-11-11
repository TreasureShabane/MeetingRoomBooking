using MeetingRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeetingRoomBooking.Infrastructure.Data;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingRoom>(entity =>
        {
            entity.HasKey(room => room.Id);
            entity.Property(room => room.Name).HasMaxLength(100).IsRequired();
            entity.Property(room => room.Capacity).IsRequired();
            entity.HasData(SeedData.Rooms.ToArray());
        });

        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.UtcTicks,
            ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.Title).HasMaxLength(120).IsRequired();
            entity.Property(booking => booking.Start).HasConversion(dateTimeOffsetConverter);
            entity.Property(booking => booking.End).HasConversion(dateTimeOffsetConverter);
            entity.HasIndex(booking => new { booking.RoomId, booking.Start, booking.End });
            entity.HasOne<MeetingRoom>()
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
