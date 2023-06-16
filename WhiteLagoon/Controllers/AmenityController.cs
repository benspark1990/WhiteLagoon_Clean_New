﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.App.ViewModels;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.App.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Amenity> AmenityList = _unitOfWork.Amenity.GetAll(includeProperties: "Villa").OrderBy(u => u.Villa.Name).ToList();
            return View(AmenityList);
        }
        public IActionResult Create()
        {
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM AmenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Add(AmenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "Amenity Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(AmenityVM);
        }

        public IActionResult Update(int amenityId)
        {
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.Id == amenityId)
            };
            if (AmenityVM.Amenity==null)
            {
                return RedirectToAction("error", "home");
            }
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Update(AmenityVM AmenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(AmenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "Amenity Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(AmenityVM);
        }

        public IActionResult Delete(int amenityId)
        {
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Amenity.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.Id == amenityId)
            };
            if (AmenityVM.Amenity == null)
            {
                return RedirectToAction("error", "home");
            }
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM AmenityVM)
        {
            if (ModelState.IsValid)
            {
                Amenity? objFromDb = _unitOfWork.Amenity.Get(x => x.Id == AmenityVM.Amenity.Id);
                if (objFromDb != null)
                {
                    _unitOfWork.Amenity.Remove(objFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Amenity Deleted Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(AmenityVM);
        }

    }
}
