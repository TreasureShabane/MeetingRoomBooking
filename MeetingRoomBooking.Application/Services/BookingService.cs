using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Exceptions;
using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Services;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    IMeetingRoomRepository meetingRoomRepository) : IBookingService
{
    public async Task<IReadOnlyCollection<BookingDto>> GetAllBookingsAsync(CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetAllAsync(cancellationToken);
        var rooms = await meetingRoomRepository.GetAllAsync(cancellationToken);
        return MapBookings(bookings, rooms);
    }

    public async Task<IReadOnlyCollection<BookingDto>> GetBookingsForRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetAllAsync(cancellationToken);
        var rooms = await meetingRoomRepository.GetAllAsync(cancellationToken);
        return MapBookings(bookings.Where(booking => booking.RoomId == roomId), rooms);
    }

    public async Task<BookingDto?> GetBookingAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var room = await meetingRoomRepository.GetByIdAsync(booking.RoomId, cancellationToken);
        return room is null ? null : Map(booking, room);
    }

    public async Task<BookingDto> CreateBookingAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var room = await ValidateAsync(request.RoomId, request.Title, request.Start, request.End, null, cancellationToken);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            Title = request.Title.Trim(),
            Start = request.Start,
            End = request.End
        };

        var created = await bookingRepository.TryCreateAsync(booking, cancellationToken);
        if (!created)
        {
            throw new BookingConflictException("The room is already booked during this time.");
        }

        return Map(booking, room);
    }

    public async Task<BookingDto> UpdateBookingAsync(
        Guid id,
        UpdateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var existingBooking = await bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Booking with id '{id}' was not found.");

        var room = await ValidateAsync(
            request.RoomId,
            request.Title,
            request.Start,
            request.End,
            id,
            cancellationToken);

        var updatedBooking = new Booking
        {
            Id = existingBooking.Id,
            RoomId = request.RoomId,
            Title = request.Title.Trim(),
            Start = request.Start,
            End = request.End
        };

        var updated = await bookingRepository.TryUpdateAsync(updatedBooking, cancellationToken);
        if (!updated)
        {
            throw new BookingConflictException("The room is already booked during this time.");
        }

        return Map(updatedBooking, room);
    }

    public async Task DeleteBookingAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await bookingRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new KeyNotFoundException($"Booking with id '{id}' was not found.");
        }
    }

    private async Task<MeetingRoom> ValidateAsync(
        Guid roomId,
        string title,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? currentBookingId,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (roomId == Guid.Empty)
        {
            errors.Add("A meeting room must be selected.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Booking title is required.");
        }
        else if (title.Trim().Length > 120)
        {
            errors.Add("Booking title cannot exceed 120 characters.");
        }

        if (start == default)
        {
            errors.Add("Booking start time is required.");
        }

        if (end == default)
        {
            errors.Add("Booking end time is required.");
        }

        if (start != default && end != default && start >= end)
        {
            errors.Add("Start time must be before end time.");
        }

        var room = roomId == Guid.Empty
            ? null
            : await meetingRoomRepository.GetByIdAsync(roomId, cancellationToken);
        if (roomId != Guid.Empty && room is null)
        {
            errors.Add("The selected room does not exist.");
        }

        if (errors.Count > 0)
        {
            throw new BookingValidationException(errors);
        }

        var hasConflict = await bookingRepository.HasOverlapAsync(
            roomId,
            start,
            end,
            currentBookingId,
            cancellationToken);
        if (hasConflict)
        {
            throw new BookingConflictException("The room is already booked during this time.");
        }

        return room ?? throw new BookingValidationException(["The selected room does not exist."]);
    }

    private static IReadOnlyCollection<BookingDto> MapBookings(
        IEnumerable<Booking> bookings,
        IEnumerable<MeetingRoom> rooms)
    {
        var roomsById = rooms.ToDictionary(room => room.Id);
        return bookings
            .OrderBy(booking => booking.Start)
            .Where(booking => roomsById.ContainsKey(booking.RoomId))
            .Select(booking => Map(booking, roomsById[booking.RoomId]))
            .ToArray();
    }

    private static BookingDto Map(Booking booking, MeetingRoom room)
    {
        return new BookingDto(
            booking.Id,
            booking.RoomId,
            room.Name,
            booking.Title,
            booking.Start,
            booking.End);
    }
}
