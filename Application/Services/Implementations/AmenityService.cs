using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementations
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Amenity Get(int id)
        {
            return _unitOfWork.Amenity.Get(u => u.Id == id);
        }

        public List<Amenity> GetAll(string? includeProperties = null)
        {
            return _unitOfWork.Amenity.GetAll(includeProperties: includeProperties).ToList();
        }

        public void Create(Amenity amenity)
        {
            if (amenity == null)
                return;

            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Save();
        }

        public void Update(Amenity amenity)
        {
            if (amenity == null)
                return;

            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Save();
        }

        public void Delete(Amenity amenity)
        {
            if (amenity == null)
                return;

            _unitOfWork.Amenity.Remove(amenity);
            _unitOfWork.Save();
        }

    }
}
