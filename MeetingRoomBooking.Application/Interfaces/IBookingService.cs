using MeetingRoomBooking.Application.Dtos;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyCollection<BookingDto>> GetAllBookingsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BookingDto>> GetBookingsForRoomAsync(Guid roomId, CancellationToken cancellationToken);
    Task<BookingDto?> GetBookingAsync(Guid id, CancellationToken cancellationToken);
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken);
    Task<BookingDto> UpdateBookingAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken);
    Task DeleteBookingAsync(Guid id, CancellationToken cancellationToken);
}
