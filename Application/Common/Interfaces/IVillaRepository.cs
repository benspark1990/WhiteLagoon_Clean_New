using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IVillaRepository : IRepository<Villa>
    {
        void Update(Villa entity);
        //public Task<bool> IsRoomBooked(int RoomId, string checkInDate, string checkOutDate);
    }
}
