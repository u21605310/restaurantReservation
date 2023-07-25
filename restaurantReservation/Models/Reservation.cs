using System.ComponentModel.DataAnnotations;

namespace restaurantReservation.Models
{
    public class Reservation
    {
        [Key]
        public int reservationId { get; set; }
        public string CustomerName { get; set; }
        public DateTime ReservationDateTime { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableNumber { get; set; }
    }
}
