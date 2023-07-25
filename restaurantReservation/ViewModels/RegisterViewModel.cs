using System.ComponentModel.DataAnnotations;

namespace restaurantReservation.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Your Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Your Surname is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string emailaddress { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string password { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string phone { get; set; }

    }
}

