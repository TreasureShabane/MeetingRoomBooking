using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;
using MeetingRoomBooking.Infrastructure.Data;

namespace MeetingRoomBooking.Infrastructure.Repositories;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings;

    public InMemoryBookingRepository()
    {
        _bookings = SeedData.Bookings.ToList();
    }

    public IReadOnlyCollection<Booking> GetAll()
    {
        return _bookings.AsReadOnly();
    }

    public Booking? GetById(Guid id)
    {
        return _bookings.FirstOrDefault(booking => booking.Id == id);
    }

    public Booking Create(Booking booking)
    {
        _bookings.Add(booking);
        return booking;
    }

    public Booking Update(Booking booking)
    {
        var index = _bookings.FindIndex(existingBooking => existingBooking.Id == booking.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Booking with id '{booking.Id}' was not found.");
        }

        _bookings[index] = booking;
        return booking;
    }

    public bool Delete(Guid id)
    {
        return _bookings.RemoveAll(booking => booking.Id == id) > 0;
    }
}
