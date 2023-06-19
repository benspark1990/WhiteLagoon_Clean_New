using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IVillaNumberService
    {
        VillaNumber Get(int number);
        List<VillaNumber> GetAll(string? includeProperties = null);
        List<VillaNumber> GetByNumber(int number);
        void Create(VillaNumber villaNumber);
        void Update(VillaNumber villaNumber);
        void Delete(VillaNumber villaNumber);
    }
}
