﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Common;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.App.Controllers
{
    public class BookingController : Controller
    {
        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);
            BookingDetail booking = new()
            {
                Villa=_unitOfWork.Villa.Get(u=>u.Id==villaId, includeProperties: "VillaAmenity"),
                CheckInDate=checkInDate,
                Nights=nights,
                CheckOutDate=checkInDate.AddDays(nights),
                UserId=userId,
                Phone = user.PhoneNumber,
            Email = user.Email,
            Name = user.Name,
            
        };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(BookingDetail bookingDetail)
        {

            var villa = _unitOfWork.Villa.Get(u => u.Id == bookingDetail.VillaId);

            bookingDetail.TotalCost = (villa.Price * bookingDetail.Nights);
            bookingDetail.Status = SD.StatusPending;
            bookingDetail.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(bookingDetail);
            _unitOfWork.Save();



                //it is a regular customer account and we need to capture payment
                //stripe logic
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
                    UnitAmount = (long)(bookingDetail.TotalCost * 100), // $20.50 => 2050
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string>()
                        //        {
                        //            Request.Scheme + "://" + Request.Host.Value + villa.ImageUrl.Replace('\\','/')
                        //        },
                        
                    }
                    
                },
                Quantity=1
            });


                var service = new SessionService();

            //RAVI check availability again to be double sure
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, villaNumbersList,
                bookingDetail.CheckInDate, bookingDetail.Nights, bookedVillas);
            if (roomsAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new {
                    villaId= bookingDetail.VillaId,
                    checkInDate = bookingDetail.CheckInDate,
                    nights = bookingDetail.Nights
                });
            }


            Session session = service.Create(options);
                _unitOfWork.Booking.UpdateStripePaymentID(bookingDetail.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status==SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId, bookingFromDb.CheckInDate);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId
                            && availableVillaNumbers.Any(x => x == m.Villa_Number)).ToList();
            }
            else
            {
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId && m.Villa_Number == bookingFromDb.VillaNumber).ToList();
            }
            return View(bookingFromDb);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.Status == SD.StatusPending)
            {
                //this is a pending order

                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved,0);
                    _unitOfWork.Save();
                }

            }
            return View(bookingId);
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCheckedIn,bookingDetail.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCompleted,0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status="")
        {
            IEnumerable<BookingDetail> objBookings;


            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking
                    .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if(!string.IsNullOrWhiteSpace(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower() == status.ToLower());
            }
            
            return Json(new { data = objBookings });
        }

        public List<int> AssignAvailableVillaNumberByVilla(int villaId, DateOnly checkInDate)
        {
            List<int> availableVillaNumbers = new List<int>();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == villaId).ToList();

            var checkedInVilla = _unitOfWork.Booking.GetAll().Where(m => m.Status==SD.StatusCheckedIn && m.VillaId == villaId).Select(u=>u.VillaNumber);


            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    //Villa is not checked in
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }
        #endregion
    }
}
