using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementations;

public class VillaService : IVillaService
{
    private readonly IUnitOfWork _unitOfWork;

    public VillaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Villa GetById(int id)
    {
        return _unitOfWork.Villa.Get(u => u.Id == id);
    }

    public List<Villa> GetAll(string? includeProperties = null)
    {
        return _unitOfWork.Villa.GetAll(includeProperties: includeProperties).ToList();
    }

    public void Create(Villa villa, string path)
    {
        if (villa == null)
            return;

        if (villa.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
            string productPath = Path.Combine(path, @"images\products");

            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                //delete the old image
                var oldImagePath =
                    Path.Combine(path, villa.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
            {
                villa.Image.CopyTo(fileStream);
            }

            villa.ImageUrl = @"\images\products\" + fileName;
        }
        else
        {
            villa.ImageUrl = "https://placehold.co/600x400";
        }


        _unitOfWork.Villa.Add(villa);
        _unitOfWork.Save();
    }

    public void Update(Villa villa, string path)
    {
        if (villa == null)
            return;

        if (villa.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
            string productPath = Path.Combine(path, @"images\products");

            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                //delete the old image
                var oldImagePath =
                    Path.Combine(path, villa.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
            {
                villa.Image.CopyTo(fileStream);
            }

            villa.ImageUrl = @"\images\products\" + fileName;
        }

        _unitOfWork.Villa.Update(villa);
        _unitOfWork.Save();
    }

    public void Delete(Villa villa, string path)
    {
        if (villa == null)
            return;

        if (!string.IsNullOrEmpty(villa.ImageUrl))
        {
            var oldImagePath =
                   Path.Combine(path, villa.ImageUrl.TrimStart('\\'));
            FileInfo file = new(oldImagePath);

            if (file.Exists)
            {
                file.Delete();
            }
        }
        _unitOfWork.Villa.Remove(villa);
        _unitOfWork.Save();
    }
}
