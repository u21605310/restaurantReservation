using Microsoft.AspNetCore.Identity;

namespace restaurantReservation.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Address
        {
            get; internal set;
        }
    }
}
