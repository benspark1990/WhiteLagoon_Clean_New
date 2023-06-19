using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IVillaService
    {
        Villa GetById(int id);
        List<Villa> GetAll(string? includeProperties = null);
        void Create(Villa villa, string path);
        void Update(Villa villa, string path);
        void Delete(Villa villa, string path);
    }
}
