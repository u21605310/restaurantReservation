using System;
using System.ComponentModel.DataAnnotations;

namespace restaurantReservation.ViewModels
{
    public class ReservationViewModel
    {
        public string CustomerName { get; set; }

        [FutureDate(ErrorMessage = "Reservation date must be in the future.")]
        public DateTime ReservationDateTime { get; set; }

        public int NumberOfGuests { get; set; }
        public int TableNumber { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateTime = (DateTime)value;
            return dateTime > DateTime.Now;
        }
    }
}

