using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IAmenityService
    {
        Amenity Get(int id);
        List<Amenity> GetAll(string? includeProperties = null);
        void Create(Amenity amenity);
        void Update(Amenity amenity);
        void Delete(Amenity amenity);
    }
}
