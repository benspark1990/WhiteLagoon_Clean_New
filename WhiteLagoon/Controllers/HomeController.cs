﻿using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.App.ViewModels;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;

namespace WhiteLagoon.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList(),
                Nights = 1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status==SD.StatusApproved ||
            u.Status==SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomsAvailable > 0 ? true : false;
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Details(int villaId)
        {
            DetailsVM detailsVM = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId),
                Nights = 1
            };
            return View(detailsVM);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}