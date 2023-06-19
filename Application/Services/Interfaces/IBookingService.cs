using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IBookingService
{
    BookingDetail FinalizeBooking(int villaId, DateOnly checkInDate, int nights, string userId);
    BookingDetail BookingDetails(int bookingId);
    void BookingConfirmation(int bookingId, string sessionId, string sessionPaymentIntentId);
    void CheckIn(BookingDetail bookingDetail);
    void CheckOut(BookingDetail bookingDetail);
    void CancelBooking(BookingDetail bookingDetail);
    BookingDetail GetBookingById(int bookingId);
    IEnumerable<BookingDetail> GetBookings();
    IEnumerable<BookingDetail> GetBookings(string userId);
    BookingDetail UpdateBookingDetails(BookingDetail bookingDetail);
    int GetAvailableRoomsCount(Villa villa, BookingDetail bookingDetail);
    void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    List<BookingDetail> GetAllByStatus();
}
