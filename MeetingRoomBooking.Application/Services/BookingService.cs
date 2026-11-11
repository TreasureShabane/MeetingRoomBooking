using MeetingRoomBooking.Application.Dtos;
using MeetingRoomBooking.Application.Exceptions;
using MeetingRoomBooking.Application.Interfaces;
using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMeetingRoomRepository _meetingRoomRepository;

    public BookingService(IBookingRepository bookingRepository, IMeetingRoomRepository meetingRoomRepository)
    {
        _bookingRepository = bookingRepository;
        _meetingRoomRepository = meetingRoomRepository;
    }

    public IReadOnlyCollection<BookingDto> GetAllBookings()
    {
        return MapBookings(_bookingRepository.GetAll());
    }

    public IReadOnlyCollection<BookingDto> GetBookingsForRoom(Guid roomId)
    {
        return MapBookings(_bookingRepository.GetAll().Where(booking => booking.RoomId == roomId));
    }

    public BookingDto? GetBooking(Guid id)
    {
        var booking = _bookingRepository.GetById(id);
        if (booking is null)
        {
            return null;
        }

        var room = _meetingRoomRepository.GetById(booking.RoomId);
        return room is null ? null : Map(booking, room);
    }

    public BookingDto CreateBooking(CreateBookingRequest request)
    {
        Validate(request.RoomId, request.Title, request.Start, request.End, null);

        var room = GetRequiredRoom(request.RoomId);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            Title = request.Title.Trim(),
            Start = request.Start,
            End = request.End
        };

        var created = _bookingRepository.Create(booking);
        return Map(created, room);
    }

    public BookingDto UpdateBooking(Guid id, UpdateBookingRequest request)
    {
        var existingBooking = _bookingRepository.GetById(id)
            ?? throw new KeyNotFoundException($"Booking with id '{id}' was not found.");

        Validate(request.RoomId, request.Title, request.Start, request.End, id);

        var room = GetRequiredRoom(request.RoomId);

        var updatedBooking = new Booking
        {
            Id = existingBooking.Id,
            RoomId = request.RoomId,
            Title = request.Title.Trim(),
            Start = request.Start,
            End = request.End
        };

        var savedBooking = _bookingRepository.Update(updatedBooking);
        return Map(savedBooking, room);
    }

    public void DeleteBooking(Guid id)
    {
        var deleted = _bookingRepository.Delete(id);
        if (!deleted)
        {
            throw new KeyNotFoundException($"Booking with id '{id}' was not found.");
        }
    }

    private void Validate(Guid roomId, string title, DateTimeOffset start, DateTimeOffset end, Guid? currentBookingId)
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

        if (roomId != Guid.Empty && _meetingRoomRepository.GetById(roomId) is null)
        {
            errors.Add("The selected room does not exist.");
        }

        if (roomId != Guid.Empty)
        {
            var hasConflict = _bookingRepository.GetAll().Any(booking =>
                booking.RoomId == roomId &&
                booking.Id != currentBookingId &&
                start < booking.End &&
                booking.Start < end);

            if (hasConflict)
            {
                errors.Add("The room is already booked during this time.");
            }
        }

        if (errors.Count > 0)
        {
            throw new BookingValidationException(errors);
        }
    }

    private MeetingRoom GetRequiredRoom(Guid roomId)
    {
        var room = _meetingRoomRepository.GetById(roomId);
        if (room is null)
        {
            throw new BookingValidationException(["The selected room does not exist."]);
        }

        return room;
    }

    private IReadOnlyCollection<BookingDto> MapBookings(IEnumerable<Booking> bookings)
    {
        var mappedBookings = new List<BookingDto>();

        foreach (var booking in bookings.OrderBy(item => item.Start))
        {
            var room = _meetingRoomRepository.GetById(booking.RoomId);
            if (room is not null)
            {
                mappedBookings.Add(Map(booking, room));
            }
        }

        return mappedBookings;
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
