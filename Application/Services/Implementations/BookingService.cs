using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Common;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVillaService _villaService;

    public BookingService(IUnitOfWork unitOfWork, IVillaService villaService)
    {
        _unitOfWork = unitOfWork;
        _villaService = villaService;
    }

    public void BookingConfirmation(int bookingId, string sessionId, string sessionPaymentIntentId)
    {
        _unitOfWork.Booking.UpdateStripePaymentID(bookingId, sessionId, sessionPaymentIntentId);
        _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved, 0);
        _unitOfWork.Save();
    }

    public BookingDetail BookingDetails(int bookingId)
    {
        BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
        if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
        {
            var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId, bookingFromDb.CheckInDate);

            bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId
                        && availableVillaNumbers.Any(x => x == m.Villa_Number)).ToList();
        }
        else
        {
            bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId && m.Villa_Number == bookingFromDb.VillaNumber).ToList();
        }

        return bookingFromDb;
    }

    public void CancelBooking(BookingDetail bookingDetail)
    {
        _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCancelled, 0);
        _unitOfWork.Save();
    }

    public void CheckIn(BookingDetail bookingDetail)
    {
        _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCheckedIn, bookingDetail.VillaNumber);
        _unitOfWork.Save();
    }

    public void CheckOut(BookingDetail bookingDetail)
    {
        _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCompleted, 0);
        _unitOfWork.Save();
    }

    public BookingDetail FinalizeBooking(int villaId, DateOnly checkInDate, int nights, string userId)
    {
        ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

        BookingDetail booking = new()
        {
            Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
            CheckInDate = checkInDate,
            Nights = nights,
            CheckOutDate = checkInDate.AddDays(nights),
            UserId = userId,
            Phone = user.PhoneNumber,
            Email = user.Email,
            Name = user.Name,

        };

        booking.TotalCost = booking.Villa.Price * nights;

        return booking;
    }

    public List<BookingDetail> GetAllByStatus()
    {
        return _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();
    }

    public int GetAvailableRoomsCount(Villa villa, BookingDetail bookingDetail)
    {
        var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
        var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved
                                                        || u.Status == SD.StatusCheckedIn)
                                                        .ToList();

        int roomsAvailable = SD.VillaRoomsAvailable_Count(
            villa,
            villaNumbersList,
            bookingDetail.CheckInDate,
            bookingDetail.Nights,
            bookedVillas);

        return roomsAvailable;
    }

    public BookingDetail GetBookingById(int bookingId)
    {
        return _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
    }

    public IEnumerable<BookingDetail> GetBookings()
    {
        return _unitOfWork.Booking.GetAll(includeProperties: "User,Villa").ToList();
    }

    public IEnumerable<BookingDetail> GetBookings(string userId)
    {
        return _unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
    }

    public BookingDetail UpdateBookingDetails(BookingDetail bookingDetail)
    {
        var villa = _villaService.GetById(bookingDetail.VillaId);

        bookingDetail.TotalCost = villa.Price * bookingDetail.Nights;
        bookingDetail.Status = SD.StatusPending;
        bookingDetail.BookingDate = DateTime.Now;

        _unitOfWork.Booking.Add(bookingDetail);
        _unitOfWork.Save();

        return bookingDetail;
    }

    public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
    {
        _unitOfWork.Booking.UpdateStripePaymentID(id, sessionId, paymentIntentId);
        _unitOfWork.Save();
    }

    private List<int> AssignAvailableVillaNumberByVilla(int villaId, DateOnly checkInDate)
    {
        List<int> availableVillaNumbers = new List<int>();

        var villaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == villaId).ToList();

        var checkedInVilla = _unitOfWork.Booking.GetAll().Where(m => m.Status == SD.StatusCheckedIn && m.VillaId == villaId).Select(u => u.VillaNumber);

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
}
