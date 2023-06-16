using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Common;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
