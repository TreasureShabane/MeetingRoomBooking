using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IBookingRepository
{
    IReadOnlyCollection<Booking> GetAll();
    Booking? GetById(Guid id);
    Booking Create(Booking booking);
    Booking Update(Booking booking);
    bool Delete(Guid id);
}
