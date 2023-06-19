using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.App.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVillaService _villaService;

        public BookingController(IBookingService bookingService, IVillaService villaService)
        {
            _bookingService = bookingService;
            _villaService=villaService;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var booking = _bookingService.FinalizeBooking(villaId, checkInDate, nights, userId);

            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(BookingDetail bookingDetail)
        {
            bookingDetail = _bookingService.UpdateBookingDetails(bookingDetail);

            var villa = _villaService.GetById(bookingDetail.VillaId);

            //it is a regular customer account and we need to capture payment
            //stripe logic
            var options = CreateStripeSessionOptions(bookingDetail, villa);
            var service = new SessionService();

            //RAVI check availability again to be double sure
            int roomsAvailable = _bookingService.GetAvailableRoomsCount(villa, bookingDetail);

            if (roomsAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = bookingDetail.VillaId,
                    checkInDate = bookingDetail.CheckInDate,
                    nights = bookingDetail.Nights
                });
            }

            Session session = service.Create(options);
            _bookingService.UpdateStripePaymentID(bookingDetail.Id, session.Id, session.PaymentIntentId);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            var bookingFromDb = _bookingService.BookingDetails(bookingId);
            return View(bookingFromDb);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            var bookingDetail = _bookingService.GetBookingById(bookingId);
            if (bookingDetail.Status == SD.StatusPending)
            {
                //this is a pending order
                var service = new SessionService();
                Session session = service.Get(bookingDetail.StripeSessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _bookingService.BookingConfirmation(bookingId, session.Id, session.PaymentIntentId);
                }
            }

            return View(bookingId);
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(BookingDetail bookingDetail)
        {
            _bookingService.CheckIn(bookingDetail);

            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(BookingDetail bookingDetail)
        {
            _bookingService.CheckOut(bookingDetail);

            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(BookingDetail bookingDetail)
        {
            _bookingService.CancelBooking(bookingDetail);

            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status = "")
        {
            IEnumerable<BookingDetail> bookingDetails;

            if (User.IsInRole(SD.Role_Admin))
            {
                bookingDetails = _bookingService.GetBookings();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                bookingDetails = _bookingService.GetBookings(userId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bookingDetails = bookingDetails.Where(u => u.Status.ToLower() == status.ToLower());
            }

            return Json(new { data = bookingDetails });
        }

        #endregion

        #region Private functions

        private SessionCreateOptions CreateStripeSessionOptions(BookingDetail bookingDetail, Villa villa)
        {
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={bookingDetail.Id}",
                CancelUrl = domain + $"booking/finalizeBooking?villaId={bookingDetail.VillaId}&checkInDate={bookingDetail.CheckInDate}&nights={bookingDetail.Nights}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(bookingDetail.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name
                    }
                },
                Quantity = 1
            });

            return options;
        }

        #endregion
    }
}
