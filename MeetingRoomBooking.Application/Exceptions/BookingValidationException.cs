namespace MeetingRoomBooking.Application.Exceptions;

public sealed class BookingValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public BookingValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}
