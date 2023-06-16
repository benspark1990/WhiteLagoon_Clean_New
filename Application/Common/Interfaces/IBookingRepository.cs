using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<BookingDetail>
    {
        void Update(BookingDetail entity);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int bookingId, string orderStatus, int villaNumber);
    }
}
