﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Utility;

namespace WhiteLagoon.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            Name = user.Name
        };
          
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
                    SuccessUrl = domain + $"booking/BookingConfirmation?id={bookingDetail.Id}",
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
                Session session = service.Create(options);
                _unitOfWork.Booking.UpdateStripePaymentID(bookingDetail.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

        }
        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            
            return View(bookingId);
        }
    }
}
