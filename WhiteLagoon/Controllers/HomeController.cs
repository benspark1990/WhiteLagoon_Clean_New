using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.App.ViewModels;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;

namespace WhiteLagoon.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly IBookingService _bookingService;

        public HomeController(IVillaService villaService, IVillaNumberService villaNumberService, IBookingService bookingService)
        {
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _bookingService = bookingService;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _villaService.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _villaService.GetAll(includeProperties: "VillaAmenity");
            var villaNumbersList = _villaNumberService.GetAll();
            var bookedVillas = _bookingService.GetAllByStatus();

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
                Villa = _villaService.GetById(villaId),
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