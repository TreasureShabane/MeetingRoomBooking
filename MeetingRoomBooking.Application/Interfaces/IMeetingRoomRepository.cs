using MeetingRoomBooking.Domain.Entities;

namespace MeetingRoomBooking.Application.Interfaces;

public interface IMeetingRoomRepository
{
    IReadOnlyCollection<MeetingRoom> GetAll();
    MeetingRoom? GetById(Guid id);
}
