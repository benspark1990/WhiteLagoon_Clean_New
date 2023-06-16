using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.App.ViewModels
{
    public class DetailsVM
    {
        public Villa? Villa { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly? CheckOutDate { get; set; }
        public int Nights { get; set; }
    }
}
