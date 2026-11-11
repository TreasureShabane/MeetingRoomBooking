using MeetingRoomBooking.Application.Dtos;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IBookingService
{
    IReadOnlyCollection<BookingDto> GetAllBookings();
    IReadOnlyCollection<BookingDto> GetBookingsForRoom(Guid roomId);
    BookingDto? GetBooking(Guid id);
    BookingDto CreateBooking(CreateBookingRequest request);
    BookingDto UpdateBooking(Guid id, UpdateBookingRequest request);
    void DeleteBooking(Guid id);
}
